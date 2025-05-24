using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET
{
    public static class KcpProtocalType
    {
        public const byte SYN = 1;
        public const byte ACK = 2;
        public const byte FIN = 3;
        public const byte MSG = 4;
        public const byte RouterReconnectSYN = 5;
        public const byte RouterReconnectACK = 6;
        public const byte RouterSYN = 7;
        public const byte RouterACK = 8;
    }
    
    /// <summary>
    /// KCP网络服务，基于UDP实现可靠传输
    /// 支持流模式传输，使用长度前缀协议[Length][Data]
    /// 提供连接管理、心跳检测、重连机制等功能
    /// 单线程运行，所有操作在Update循环中处理
    /// </summary>
    public sealed class KService: AService
    {
        public const int ConnectTimeoutTime = 20 * 1000;

        /// <summary>
        /// KCP指针到Channel的映射，用于KCP回调函数快速查找Channel
        /// </summary>
        public readonly Dictionary<IntPtr, KChannel> KcpPtrChannels = new();
        
        /// <summary>
        /// 服务启动时间，用于计算相对时间戳
        /// </summary>
        private readonly long startTime;

        /// <summary>
        /// 当前相对时间戳，KCP内部使用，线程安全
        /// </summary>
        public uint TimeNow => (uint) (TimeHelper.ClientNow() - this.startTime);

        private Socket socket;


#region 回调方法

        static KService()
        {
            //Kcp.KcpSetLog(KcpLog);
            Kcp.KcpSetoutput(KcpOutput);
        }
        
#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(KcpOutput))]
#endif
        private static void KcpLog(IntPtr bytes, int len, IntPtr kcp, IntPtr user)
        {
            try
            {
                unsafe
                {
                    Span<byte> span = new Span<byte>(bytes.ToPointer(), len);
                    Log.GetLogger().Information(span.ToString());
                }
            }
            catch (Exception e)
            {
                Log.GetLogger().Error(e);
            }
        }

#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(KcpOutput))]
#endif
        private static int KcpOutput(IntPtr bytes, int len, IntPtr kcp, IntPtr user)
        {
            try
            {
                if (kcp == IntPtr.Zero)
                {
                    return 0;
                }
                
                if (NetServices.Instance.Get(user.ToInt32()) is not KService kService)
                {
                    return 0;
                }
                
                if (!kService.KcpPtrChannels.TryGetValue(kcp, out KChannel kChannel))
                {
                    return 0;
                }
                
                kChannel.Output(bytes, len);
            }
            catch (Exception e)
            {
                Log.GetLogger().Error(e);
                return len;
            }

            return len;
        }

#endregion

        public KService(IPEndPoint ipEndPoint, ServiceType serviceType)
        {
            this.ServiceType = serviceType;
            this.startTime = TimeHelper.ClientNow();
            this.socket = new Socket(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                this.socket.SendBufferSize = Kcp.OneM * 64;
                this.socket.ReceiveBufferSize = Kcp.OneM * 64;
            }

            try
            {
                this.socket.Bind(ipEndPoint);
            }
            catch (Exception e)
            {
                throw new Exception($"bind error: {ipEndPoint}", e);
            }

            NetworkHelper.SetSioUdpConnReset(this.socket);
        }

        public KService(AddressFamily addressFamily, ServiceType serviceType)
        {
            this.ServiceType = serviceType;
            this.startTime = TimeHelper.ClientNow();
            this.socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);

            NetworkHelper.SetSioUdpConnReset(this.socket);
        }

        // 保存所有的channel
        /// <summary>
        /// 本地连接ID到Channel的映射，管理所有活跃连接
        /// </summary>
        private readonly Dictionary<long, KChannel> localConnChannels = new();
        /// <summary>
        /// 等待Accept确认的连接，用于处理三次握手过程中的连接
        /// </summary>
        private readonly Dictionary<long, KChannel> waitAcceptChannels = new();

        /// <summary>
        /// 网络数据收发缓存，使用ArrayPool租用以减少GC压力
        /// </summary>
        private byte[] cache;
        private EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

        /// <summary>
        /// 需要在下一帧更新的Channel ID集合
        /// </summary>
        private readonly HashSet<long> updateIds = new HashSet<long>();
        
        /// <summary>
        /// 按时间排序的Channel更新队列，支持定时器功能
        /// </summary>
        private readonly MultiMap<long, long> timeId = new MultiMap<long, long>();
        private readonly List<long> timeOutTime = new List<long>();
        /// <summary>
        /// 最近更新时间，优化时间查询性能
        /// </summary>
        private long minTime;
        
        public override bool IsDispose()
        {
            return this.socket == null;
        }

        public override void Dispose()
        {
            base.Dispose();
            
            foreach (long channelId in this.localConnChannels.Keys.ToArray())
            {
                this.Remove(channelId);
            }

            this.socket.Close();
            this.socket = null;
            
            // 归还缓存
            if (this.cache != null)
            {
                MemoryStreamPool.ReturnBuffer(this.cache);
                this.cache = null;
            }
        }

        public override (uint, uint) GetChannelConn(long channelId)
        {
            KChannel kChannel = this.Get(channelId);
            if (kChannel == null)
            {
                throw new Exception($"GetChannelConn conn not found KChannel! {channelId}");
            }
            return (kChannel.LocalConn, kChannel.RemoteConn);
        }
        
        public override void ChangeAddress(long channelId, IPEndPoint newIPEndPoint)
        {
            KChannel kChannel = this.Get(channelId);
            if (kChannel == null)
            {
                return;
            }
            kChannel.RemoteAddress = newIPEndPoint;
        }

        private IPEndPoint CloneAddress()
        {
            IPEndPoint ip = (IPEndPoint) this.ipEndPoint;
            return new IPEndPoint(ip.Address, ip.Port);
        }

        private void Recv()
        {
            if (this.socket == null)
            {
                return;
            }

            // 延迟初始化缓存
            this.cache ??= MemoryStreamPool.RentBuffer(2048);

            while (socket != null && this.socket.Available > 0)
            {
                int messageLength = this.socket.ReceiveFrom(this.cache, ref this.ipEndPoint);

                // 长度小于1，不是正常的消息
                if (messageLength < 1)
                {
                    continue;
                }

                // accept
                byte flag = this.cache[0];
                
                // conn从100开始，如果为1，2，3则是特殊包
                uint remoteConn = 0;
                uint localConn = 0;
                
                try
                {
                    KChannel kChannel = null;
                    switch (flag)
                    {
                        case KcpProtocalType.RouterReconnectSYN:
                        {
                            // 长度!=5，不是RouterReconnectSYN消息
                            if (messageLength != 13)
                            {
                                break;
                            }

                            string realAddress = null;
                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            localConn = BitConverter.ToUInt32(this.cache, 5);
                            uint connectId = BitConverter.ToUInt32(this.cache, 9);

                            this.localConnChannels.TryGetValue(localConn, out kChannel);
                            if (kChannel == null)
                            {
                                Log.GetLogger().Warning($"kchannel reconnect not found channel: {localConn} {remoteConn} {realAddress}");
                                break;
                            }

                            // 这里必须校验localConn，客户端重连，localConn一定是一样的
                            if (localConn != kChannel.LocalConn)
                            {
                                Log.GetLogger().Warning($"kchannel reconnect localconn error: {localConn} {remoteConn} {realAddress} {kChannel.LocalConn}");
                                break;
                            }

                            if (remoteConn != kChannel.RemoteConn)
                            {
                                Log.GetLogger().Warning($"kchannel reconnect remoteconn error: {localConn} {remoteConn} {realAddress} {kChannel.RemoteConn}");
                                break;
                            }

                            // 重连的时候router地址变化, 这个不能放到msg中，必须经过严格的验证才能切换
                            if (!Equals(kChannel.RemoteAddress, this.ipEndPoint))
                            {
                                kChannel.RemoteAddress = this.CloneAddress();
                            }

                            try
                            {
                                var buffer = this.cache.AsSpan();
                                buffer[0] = KcpProtocalType.RouterReconnectACK;
                                BitConverter.TryWriteBytes(buffer[1..], kChannel.LocalConn);
                                BitConverter.TryWriteBytes(buffer[5..], kChannel.RemoteConn);
                                BitConverter.TryWriteBytes(buffer[9..], connectId);
                                this.socket.SendTo(buffer[..13], SocketFlags.None, this.ipEndPoint);
                            }
                            catch (Exception e)
                            {
                                Log.GetLogger().Error(e);
                                kChannel.OnError(ErrorCore.ERR_SocketCantSend);
                            }

                            break;
                        }
                        case KcpProtocalType.SYN: // accept
                        {
                            // 长度!=5，不是SYN消息
                            if (messageLength < 9)
                            {
                                break;
                            }

                            string realAddress = null;
                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            if (messageLength > 9)
                            {
                                realAddress = this.cache.ToStr(9, messageLength - 9);
                            }

                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            localConn = BitConverter.ToUInt32(this.cache, 5);

                            this.waitAcceptChannels.TryGetValue(remoteConn, out kChannel);
                            if (kChannel == null)
                            {
                                // accept的localConn不能与connect的localConn冲突，所以设置为一个大的数
                                // localConn被人猜出来问题不大，因为remoteConn是随机的,第三方并不知道
                                localConn = NetServices.Instance.CreateAcceptChannelId();
                                // 已存在同样的localConn，则不处理，等待下次sync
                                if (this.localConnChannels.ContainsKey(localConn))
                                {
                                    break;
                                }

                                kChannel = new KChannel(localConn, remoteConn, this.socket, this.CloneAddress(), this);
                                this.waitAcceptChannels.Add(kChannel.RemoteConn, kChannel); // 连接上了或者超时后会删除
                                this.localConnChannels.Add(kChannel.LocalConn, kChannel);
                                
                                kChannel.RealAddress = realAddress;

                                IPEndPoint realEndPoint = kChannel.RealAddress == null? kChannel.RemoteAddress : NetworkHelper.ToIPEndPoint(kChannel.RealAddress);
                                NetServices.Instance.OnAccept(this.Id, kChannel.Id, realEndPoint);
                            }
                            if (kChannel.RemoteConn != remoteConn)
                            {
                                break;
                            }

                            // 地址跟上次的不一致则跳过
                            if (kChannel.RealAddress != realAddress)
                            {
                                Log.GetLogger().Error($"kchannel syn address diff: {kChannel.Id} {kChannel.RealAddress} {realAddress}");
                                break;
                            }

                            try
                            {
                                var buffer = this.cache.AsSpan();
                                buffer[0] = KcpProtocalType.ACK;
                                BitConverter.TryWriteBytes(buffer[1..], kChannel.LocalConn);
                                BitConverter.TryWriteBytes(buffer[5..], kChannel.RemoteConn);
                                Log.GetLogger().Information($"kservice syn: {kChannel.Id} {remoteConn} {localConn}");
                                this.socket.SendTo(buffer[..9], SocketFlags.None, kChannel.RemoteAddress);
                            }
                            catch (Exception e)
                            {
                                Log.GetLogger().Error(e);
                                kChannel.OnError(ErrorCore.ERR_SocketCantSend);
                            }

                            break;
                        }
                        case KcpProtocalType.ACK: // connect返回
                            // 长度!=9，不是connect消息
                            if (messageLength != 9)
                            {
                                break;
                            }

                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            localConn = BitConverter.ToUInt32(this.cache, 5);
                            kChannel = this.Get(localConn);
                            if (kChannel != null)
                            {
                                Log.GetLogger().Information($"kservice ack: {localConn} {remoteConn}");
                                kChannel.RemoteConn = remoteConn;
                                kChannel.HandleConnnect();
                            }

                            break;
                        case KcpProtocalType.FIN: // 断开
                            // 长度!=13，不是DisConnect消息
                            if (messageLength != 13)
                            {
                                break;
                            }

                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            localConn = BitConverter.ToUInt32(this.cache, 5);
                            int error = BitConverter.ToInt32(this.cache, 9);

                            // 处理chanel
                            kChannel = this.Get(localConn);
                            if (kChannel == null)
                            {
                                break;
                            }
                            
                            // 校验remoteConn，防止第三方攻击
                            if (kChannel.RemoteConn != remoteConn)
                            {
                                break;
                            }
                            
                            Log.GetLogger().Information($"kservice recv fin: {localConn} {remoteConn} {error}");
                            kChannel.OnError(ErrorCore.ERR_PeerDisconnect);

                            break;
                        case KcpProtocalType.MSG: // 断开
                            // 长度<9，不是Msg消息
                            if (messageLength < 9)
                            {
                                break;
                            }
                            // 处理chanel
                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            localConn = BitConverter.ToUInt32(this.cache, 5);

                            kChannel = this.Get(localConn);
                            if (kChannel == null)
                            {
                                // 通知对方断开
                                this.Disconnect(localConn, remoteConn, ErrorCore.ERR_KcpNotFoundChannel, (IPEndPoint) this.ipEndPoint, 1);
                                break;
                            }
                            
                            // 校验remoteConn，防止第三方攻击
                            if (kChannel.RemoteConn != remoteConn)
                            {
                                break;
                            }

                            // 对方发来msg，说明kchannel连接完成
                            if (!kChannel.IsConnected)
                            {
                                kChannel.IsConnected = true;
                                this.waitAcceptChannels.Remove(kChannel.RemoteConn);
                            }

                            kChannel.HandleRecv(this.cache, 5, messageLength - 5);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.GetLogger().Error($"kservice error: {flag} {remoteConn} {localConn}\n{e}");
                }
            }
        }

        public KChannel Get(long id)
        {
            this.localConnChannels.TryGetValue(id, out KChannel channel);
            return channel;
        }

        public override void Create(long id, IPEndPoint address)
        {
            if (this.localConnChannels.ContainsKey(id))
            {
                return;
            }

            try
            {
                // 低32bit是localConn
                uint localConn = (uint)id;
                KChannel kChannel = new KChannel(localConn, this.socket, address, this);
                this.localConnChannels.Add(kChannel.LocalConn, kChannel);
                this.AddToUpdate(this.TimeNow, kChannel.Id);
            }
            catch (Exception e)
            {
                Log.GetLogger().Error($"kservice get error: {id}\n{e}");
            }
        }

        public override void Remove(long id, int error = 0)
        {
            if (this.localConnChannels.TryGetValue(id, out KChannel kChannel))
            {
                this.localConnChannels.Remove(id);
                kChannel.Error = error;
                kChannel.Dispose();
            }
            
            this.waitAcceptChannels.Remove(id);
        }

        public void Disconnect(uint localConn, uint remoteConn, int error, IPEndPoint address, int times)
        {
            try
            {
                this.cache ??= MemoryStreamPool.RentBuffer(2048);
                
                var buffer = this.cache.AsSpan();
                for (int i = 0; i < times; ++i)
                {
                    buffer[0] = KcpProtocalType.FIN;
                    BitConverter.TryWriteBytes(buffer[1..], localConn);
                    BitConverter.TryWriteBytes(buffer[5..], remoteConn);
                    BitConverter.TryWriteBytes(buffer[9..], error);
                    this.socket.SendTo(buffer[..13], SocketFlags.None, address);
                }
            }
            catch (Exception e)
            {
                Log.GetLogger().Error($"Disconnect error {localConn} {remoteConn} {error} {address} {e}");
            }
            
            Log.GetLogger().Information($"channel send fin: {localConn} {remoteConn} {address} {error}");
        }
        
        public override void Send(long channelId, long actorId, object message)
        {
            KChannel channel = this.Get(channelId);
            if (channel == null)
            {
                return;
            }
            
            MemoryStream memoryStream = this.GetMemoryStream(message);
            channel.Send(actorId, memoryStream);
        }

        public override void Update()
        {
            uint timeNow = this.TimeNow;
            
            this.TimerOut(timeNow);
            
            this.Recv();

            this.CheckWaitAcceptChannel(timeNow);

            this.UpdateChannel(timeNow);
        }

        private readonly List<KChannel> removeWaitAcceptChannels = new List<KChannel>();
        private void CheckWaitAcceptChannel(uint timeNow)
        {
            this.removeWaitAcceptChannels.Clear();
            foreach (var kv in this.waitAcceptChannels)
            {
                KChannel kChannel = kv.Value;
                if (kChannel.IsDisposed)
                {
                    continue;
                }

                if (kChannel.IsConnected)
                {
                    continue;
                }

                if (timeNow < kChannel.CreateTime + ConnectTimeoutTime)
                {
                    continue;
                }

                this.removeWaitAcceptChannels.Add(kChannel);
            }

            foreach (KChannel kChannel in this.removeWaitAcceptChannels)
            {
                kChannel.OnError(ErrorCore.ERR_KcpAcceptTimeout);
            }
        }

        private void UpdateChannel(uint timeNow)
        {
            foreach (long id in this.updateIds)
            {
                KChannel kChannel = this.Get(id);
                if (kChannel == null)
                {
                    continue;
                }

                if (kChannel.Id == 0)
                {
                    continue;
                }

                kChannel.Update(timeNow);
            }
            this.updateIds.Clear();
        }
        
        // 服务端需要看channel的update时间是否已到
        public void AddToUpdate(long time, long id)
        {
            if (time == 0)
            {
                this.updateIds.Add(id);
                return;
            }
            if (time < this.minTime)
            {
                this.minTime = time;
            }
            this.timeId.Add(time, id);
        }
        
        // 计算到期需要update的channel
        private void TimerOut(uint timeNow)
        {
            if (this.timeId.Count == 0)
            {
                return;
            }
            

            if (timeNow < this.minTime)
            {
                return;
            }

            this.timeOutTime.Clear();

            foreach (KeyValuePair<long, List<long>> kv in this.timeId)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    minTime = k;
                    break;
                }

                this.timeOutTime.Add(k);
            }

            foreach (long k in this.timeOutTime)
            {
                foreach (long v in this.timeId[k])
                {
                    this.updateIds.Add(v);
                }
                this.timeId.Remove(k);
            }
        }
    }
}
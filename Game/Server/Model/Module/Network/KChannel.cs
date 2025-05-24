using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET
{
	public struct KcpWaitPacket
	{
		public long ActorId;
		public MemoryStream MemoryStream;
	}
	
	/// <summary>
	/// KCP网络通道，管理单个KCP连接的生命周期
	/// 支持流模式传输，使用长度前缀协议进行数据包分割
	/// 提供连接建立、数据收发、心跳维护、错误处理等功能
	/// 内部使用循环缓冲区和数据包解析器处理流式数据
	/// </summary>
	public class KChannel : AChannel
	{
		public readonly KService Service;
		
		private Socket socket;

		public IntPtr kcp { get; private set; }

		private readonly Queue<KcpWaitPacket> sendBuffer = new Queue<KcpWaitPacket>();
		
		public readonly uint CreateTime;

		public uint LocalConn
		{
			get => (uint)this.Id;
			private set => this.Id = value;
		}
		public uint RemoteConn { get; set; }

		private byte[] sendCache;
		
		public bool IsConnected { get; set; }

		public string RealAddress { get; set; }
		
		/// <summary>
		/// 流模式数据处理：循环缓冲区用于暂存接收的流数据
		/// </summary>
		private readonly CircularBuffer recvBuffer = new();
		/// <summary>
		/// 数据包解析器，从流数据中解析出完整的消息包
		/// </summary>
		private readonly PacketParser packetParser;
		
		/// <summary>
		/// 初始化KCP参数并开启流模式
		/// 根据服务类型配置不同的传输参数以适应内外网环境
		/// </summary>
		private void InitKcp()
		{
			this.Service.KcpPtrChannels.Add(this.kcp, this);
			
			switch (this.Service.ServiceType)
			{
				case ServiceType.Inner:
					Kcp.KcpNodelay(kcp, 1, 10, 2, 1);
					Kcp.KcpWndsize(kcp, 1024, 1024);
					Kcp.KcpSetmtu(kcp, 1400);
					Kcp.KcpSetminrto(kcp, 30);
					break;
				case ServiceType.Outer:
					Kcp.KcpNodelay(kcp, 1, 10, 2, 1);
					Kcp.KcpWndsize(kcp, 256, 256);
					Kcp.KcpSetmtu(kcp, 470);
					Kcp.KcpSetminrto(kcp, 30);
					break;
			}
			
			// 开启KCP流模式，支持大数据包的流式传输
			Kcp.KcpSetstream(kcp, 1);
		}
		
		/// <summary>
		/// 客户端连接构造函数
		/// </summary>
		public KChannel(uint localConn, Socket socket, IPEndPoint remoteEndPoint, KService kService)
		{
			this.LocalConn = localConn;
			this.ChannelType = ChannelType.Connect;
			
			Log.GetLogger().Information($"channel create: {this.LocalConn} {remoteEndPoint} {this.ChannelType}");
			
			this.kcp = IntPtr.Zero;
			this.Service = kService;
			this.RemoteAddress = remoteEndPoint;
			this.socket = socket;
			this.CreateTime = kService.TimeNow;
			
			// 从ArrayPool租用发送缓存，避免内存分配
			this.sendCache = MemoryStreamPool.RentBuffer(2048);
			
			// 初始化流模式数据处理组件
			this.packetParser = new PacketParser(this.recvBuffer, kService);

			this.Connect(this.CreateTime);
		}

		/// <summary>
		/// 服务端接受连接构造函数
		/// </summary>
		public KChannel(uint localConn, uint remoteConn, Socket socket, IPEndPoint remoteEndPoint, KService kService)
		{
			this.ChannelType = ChannelType.Accept;
			
			Log.GetLogger().Information($"channel create: {localConn} {remoteConn} {remoteEndPoint} {this.ChannelType}");

			this.Service = kService;
			this.LocalConn = localConn;
			this.RemoteConn = remoteConn;
			this.RemoteAddress = remoteEndPoint;
			this.socket = socket;
			this.kcp = Kcp.KcpCreate(this.RemoteConn, new IntPtr(this.Service.Id));
			this.InitKcp();
			
			// 从ArrayPool租用发送缓存，避免内存分配
			this.sendCache = MemoryStreamPool.RentBuffer(2048);
			
			// 初始化流模式数据处理组件
			this.packetParser = new PacketParser(this.recvBuffer, kService);
			
			this.CreateTime = kService.TimeNow;
		}
	

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			uint localConn = this.LocalConn;
			uint remoteConn = this.RemoteConn;
			Log.GetLogger().Information($"channel dispose: {localConn} {remoteConn} {this.Error}");
			
			long id = this.Id;
			this.Id = 0;
			this.Service.Remove(id);

			try
			{
				if (this.Error != ErrorCore.ERR_PeerDisconnect)
				{
					this.Service.Disconnect(localConn, remoteConn, this.Error, this.RemoteAddress, 3);
				}
			}
			catch (Exception e)
			{
				Log.GetLogger().Error(e);
			}

			if (this.kcp != IntPtr.Zero)
			{
				Kcp.KcpRelease(this.kcp);
				this.kcp = IntPtr.Zero;
			}

			// 归还ArrayPool租用的内存
			if (this.sendCache != null)
			{
				MemoryStreamPool.ReturnBuffer(this.sendCache);
				this.sendCache = null;
			}
			
			// 释放流资源
			this.recvBuffer?.Dispose();
			
			this.socket = null;
		}

		public void HandleConnnect()
		{
			if (this.IsConnected)
			{
				return;
			}

			this.kcp = Kcp.KcpCreate(this.RemoteConn, new IntPtr(this.Service.Id));
			this.InitKcp();

			Log.GetLogger().Information($"channel connected: {this.LocalConn} {this.RemoteConn} {this.RemoteAddress}");
			this.IsConnected = true;
			
			while (this.sendBuffer.Count > 0)
			{
				KcpWaitPacket buffer = this.sendBuffer.Dequeue();
				this.KcpSend(buffer);
			}
		}

		/// <summary>
		/// 发送请求连接消息
		/// </summary>
		private void Connect(uint timeNow)
		{
			try
			{
				if (this.IsConnected)
				{
					return;
				}
				
				if (timeNow > this.CreateTime + KService.ConnectTimeoutTime)
				{
					Log.GetLogger().Error($"kChannel connect timeout: {this.Id} {this.RemoteConn} {timeNow} {this.CreateTime} {this.ChannelType} {this.RemoteAddress}");
					this.OnError(ErrorCore.ERR_KcpConnectTimeout);
					return;
				}
				
				var buffer = this.sendCache.AsSpan();
				buffer[0] = KcpProtocalType.SYN;
				BitConverter.TryWriteBytes(buffer[1..], this.LocalConn);
				BitConverter.TryWriteBytes(buffer[5..], this.RemoteConn);
				
				this.socket.SendTo(buffer[..9], SocketFlags.None, this.RemoteAddress);
				Log.GetLogger().Information($"kchannel connect {this.LocalConn} {this.RemoteConn} {this.RealAddress} {this.socket.LocalEndPoint}");
				
				this.Service.AddToUpdate(timeNow + 300, this.Id);
			}
			catch (Exception e)
			{
				Log.GetLogger().Error(e);
				this.OnError(ErrorCore.ERR_SocketCantSend);
			}
		}

		public void Update(uint timeNow)
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			if (!this.IsConnected && this.ChannelType == ChannelType.Connect)
			{
				this.Connect(timeNow);
				return;
			}

			if (this.kcp == IntPtr.Zero)
			{
				return;
			}
			
			try
			{
				Kcp.KcpUpdate(this.kcp, timeNow);
			}
			catch (Exception e)
			{
				Log.GetLogger().Error(e);
				this.OnError(ErrorCore.ERR_SocketError);
				return;
			}

			uint nextUpdateTime = Kcp.KcpCheck(this.kcp, timeNow);
			this.Service.AddToUpdate(nextUpdateTime, this.Id);
		}

		public void HandleRecv(byte[] data, int offset, int length)
		{
			if (this.IsDisposed)
			{
				return;
			}

			try
			{
				Kcp.KcpInput(this.kcp, data, offset, length);
			}
			catch (Exception e)
			{
				Log.GetLogger().Error(e);
				this.OnError(ErrorCore.ERR_SocketError);
				return;
			}
			
			// 流模式：从KCP读取数据到循环缓冲区
			while (true)
			{
				if (this.kcp == IntPtr.Zero)
				{
					Log.GetLogger().Error($"kcp is zero");
					return;
				}
				
				int messageSize = Kcp.KcpPeeksize(this.kcp);
				if (messageSize < 0)
				{
					break; // 没有更多数据
				}

				if (messageSize == 0)
				{
					break; // 暂时没有完整数据包
				}

				// 使用ArrayPool租用临时缓冲区
				var tempBuffer = MemoryStreamPool.RentBuffer(messageSize);
				try
				{
					int realSize = Kcp.KcpRecv(this.kcp, tempBuffer, 0, messageSize);
					if (realSize != messageSize)
					{
						Log.GetLogger().Error($"kchannel recv error: {realSize} {messageSize} {this.LocalConn} {this.RemoteConn}");
						this.OnError(ErrorCore.ERR_KcpRecvError);
						return;
					}

					// 将数据写入循环缓冲区
					this.recvBuffer.Write(tempBuffer, 0, realSize);
				}
				finally
				{
					MemoryStreamPool.ReturnBuffer(tempBuffer);
				}
			}
			
			// 使用PacketParser解析带长度前缀的数据包
			while (this.packetParser.Parse())
			{
				MemoryStream memoryStream = this.packetParser.MemoryStream;
				try
				{
					this.OnRead(memoryStream);
				}
				finally
				{
					// 回收MemoryStream
					MemoryStreamPool.Instance.Recycle(memoryStream);
				}
			}
		}

		public void Output(IntPtr bytes, int count)
		{
			if (this.IsDisposed)
			{
				return;
			}
			try
			{				
				if (!this.IsConnected)
				{
					return;
				}
				
				if (count == 0)
				{
					Log.GetLogger().Error($"output 0");
					return;
				}

				var buffer = this.sendCache.AsSpan();
				buffer[0] = KcpProtocalType.MSG;
				BitConverter.TryWriteBytes(buffer[1..], this.LocalConn);

				unsafe
				{
					var dataSpan = new ReadOnlySpan<byte>(bytes.ToPointer(), count);
					dataSpan.CopyTo(buffer[5..]);
				}
				
				this.socket.SendTo(buffer[..(count + 5)], SocketFlags.None, this.RemoteAddress);
			}
			catch (Exception e)
			{
				Log.GetLogger().Error(e);
			}
		}

		// 流模式发送：先发送长度，再发送数据
		private void KcpSend(KcpWaitPacket kcpWaitPacket)
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			MemoryStream memoryStream = kcpWaitPacket.MemoryStream;
			
			switch (this.Service.ServiceType)
			{
				case ServiceType.Inner:
				{
					var buffer = memoryStream.GetBuffer().AsSpan();
					BitConverter.TryWriteBytes(buffer, kcpWaitPacket.ActorId);
					break;
				}
				case ServiceType.Outer:
				{
					memoryStream.Seek(Packet.ActorIdLength, SeekOrigin.Begin);
					break;
				}
			}
			
			int dataLength = (int)(memoryStream.Length - memoryStream.Position);
			
			// 流模式：统一使用4字节长度前缀（内外网一致）
			var lengthBuffer = this.sendCache.AsSpan(0, PacketParser.InnerPacketSizeLength);
			BitConverter.TryWriteBytes(lengthBuffer, dataLength);
			Kcp.KcpSend(this.kcp, this.sendCache, 0, PacketParser.InnerPacketSizeLength);
			
			// 然后发送实际数据
			Kcp.KcpSend(this.kcp, memoryStream.GetBuffer(), (int)memoryStream.Position, dataLength);
			
			this.Service.AddToUpdate(0, this.Id);
		}
		
		public void Send(long actorId, MemoryStream stream)
		{
			KcpWaitPacket kcpWaitPacket = new KcpWaitPacket() { ActorId = actorId, MemoryStream = stream };
			
			if (!this.IsConnected)
			{
				this.sendBuffer.Enqueue(kcpWaitPacket);
				return;
			}
			
			if (this.kcp == IntPtr.Zero)
			{
				throw new Exception("kchannel connected but kcp is zero!");
			}
			
			int n = Kcp.KcpWaitsnd(this.kcp);
			int maxWaitSize = this.Service.ServiceType switch
			{
				ServiceType.Inner => Kcp.InnerMaxWaitSize,
				ServiceType.Outer => Kcp.OuterMaxWaitSize,
				_ => throw new ArgumentOutOfRangeException()
			};
			
			if (n > maxWaitSize)
			{
				Log.GetLogger().Error($"kcp wait snd too large: {n}: {this.LocalConn} {this.RemoteConn}");
				this.OnError(ErrorCore.ERR_KcpWaitSendSizeTooLarge);
				return;
			}
			
			this.KcpSend(kcpWaitPacket);
		}
		
		private void OnRead(MemoryStream memoryStream)
		{
			try
			{
				long channelId = this.Id;
				object message = null;
				long actorId = 0;
				
				switch (this.Service.ServiceType)
				{
					case ServiceType.Outer:
					{
						ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.KcpOpcodeIndex);
						Type type = NetServices.Instance.GetType(opcode);
						message = SerializeHelper.Deserialize(opcode, type, memoryStream);
						break;
					}
					case ServiceType.Inner:
					{
						actorId = BitConverter.ToInt64(memoryStream.GetBuffer(), Packet.ActorIdIndex);
						ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
						Type type = NetServices.Instance.GetType(opcode);
						message = SerializeHelper.Deserialize(opcode, type, memoryStream);
						break;
					}
				}
				
				NetServices.Instance.OnRead(this.Service.Id, channelId, actorId, message);
			}
			catch (Exception e)
			{
				Log.GetLogger().Error(e);
				this.OnError(ErrorCore.ERR_PacketParserError);
			}
		}
		
		public void OnError(int error)
		{
			long channelId = this.Id;
			this.Service.Remove(channelId, error);
			NetServices.Instance.OnError(this.Service.Id, channelId, error);
		}
	}
}

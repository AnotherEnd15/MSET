using System;
using System.Collections.Generic;


namespace ET
{
    public enum NetworkProtocol
    {
        TCP,
        KCP,
        Websocket,
    }

    public enum NetOp: byte
    {
        AddService = 1,
        RemoveService = 2,
        OnAccept = 3,
        OnRead = 4,
        OnError = 5,
        CreateChannel = 6,
        RemoveChannel = 7,
        SendMessage = 9,
        GetChannelConn = 10,
        ChangeAddress = 11,
    }

    public struct NetOperator
    {
        public NetOp Op; // 操作码
        public int ServiceId;
        public long ChannelId;
        public long ActorId;
        public object Object; // 参数
    }

    public class NetServices: Singleton<NetServices>
    {
        public NetServices()
        {
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof (MessageAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
                if (messageAttribute == null)
                {
                    continue;
                }

                this.typeOpcode.Add(type, messageAttribute.Opcode);
            }
        }

        #region 线程安全

        // 初始化后不变，所以主线程，网络线程都可以读
        private readonly DoubleMap<Type, ushort> typeOpcode = new DoubleMap<Type, ushort>();

        public ushort GetOpcode(Type type)
        {
            return this.typeOpcode.GetValueByKey(type);
        }

        public Type GetType(ushort opcode)
        {
            return this.typeOpcode.GetKeyByValue(opcode);
        }

        #endregion

        private readonly Dictionary<long, WChannel> channels = new();

        private int channelIdGenerator = 10000;

        public WChannel AddChannel(string address,Action<long,int> onError)
        {
            var id = channelIdGenerator++;
            var channel = new WChannel(id,address,e=>onError.Invoke(id,e));
            this.channels[id] = channel;
            return channel;
        }

        public void RemoveChannel(long serviceId, long channelId,int error)
        {
            this.channels.TryGetValue(channelId,out var channel);
            channel?.Dispose();
            this.channels.Remove(channelId);
        }

        public void SendMessage(long serviceId, long channelId, long actorId, object message)
        {
            if (channels.TryGetValue(channelId, out var channel))
            {
                var result = MessageSerializeHelper.MessageToStream(message);
                channel.Send(result.Item2);
            }
        }

        public void OnError(long channelId, int error)
        {
            this.RemoveChannel(0,channelId,error);
        }
        
        
        private readonly Dictionary<long, Action<long,object>> readCallback = new();
        private readonly Dictionary<long, Action<long, int>> errorCallback = new ();
        
        public void RegisterReadCallback(long channelId, Action<long,object> action)
        {
            this.readCallback.Add(channelId, action);
        }
        
        public void RegisterErrorCallback(long channelId, Action<long, int> action)
        {
            this.errorCallback.Add(channelId, action);
        }
        
        public void OnRead(long channelId, object message)
        {
            if(this.readCallback.TryGetValue(channelId,out var act))
            {
                act.Invoke(channelId,message);
            }
        }
    }
}
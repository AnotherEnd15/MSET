
using ET.EventType;

namespace ET
{
    [FriendOf(typeof(NetClientComponent))]
    public static class NetClientComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<NetClientComponent, string>
        {
            protected override void Awake(NetClientComponent self, string address)
            {
                self.CreateSession(address);
                self.SetClientCache();
            }
        }

        [ObjectSystem]
        public class DestroySystem: DestroySystem<NetClientComponent>
        {
            protected override void Destroy(NetClientComponent self)
            {
               
            }
        }

        public static void CreateSession(this NetClientComponent self,string address)
        {
            if (self.Session != null)
            {
                self.Session.Dispose();
                self.Session = null;
            }
            
            self.WChannel = NetServices.Instance.AddChannel(address,self.OnError);
            NetServices.Instance.RegisterReadCallback(self.WChannel.Id, self.OnRead);
            NetServices.Instance.RegisterErrorCallback(self.WChannel.Id, self.OnError);
            self.Session = self.AddChildWithId<Session, int>(self.WChannel.Id, 0);
            self.Session.AddComponent<SessionIdleCheckerComponent>();
        }

        // 让客户端继承之前的数据
        public static void SetClientCache(this NetClientComponent self)
        {
            self.WChannel.SetBuffer(self.Sendbuffer);
            self.Session.requestCallbacks = self.RequestCallbacks;
        }

        private static void OnRead(this NetClientComponent self, long channelId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }
            session.LastRecvTime = TimeHelper.ClientNow();
            
            OpcodeHelper.LogMsg(self.DomainZone(), message);
            
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetClientComponentOnRead() {Session = session, Message = message});
        }

        private static void OnError(this NetClientComponent self, long channelId, int error)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.Error = error;
            session.Dispose();
            
            EventSystem.Instance.Publish(self.DomainScene(),new OnDisconnect(){error = error});
        }
    }
}
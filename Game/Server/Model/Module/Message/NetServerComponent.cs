using System.Net;

namespace ET
{
    public struct NetServerComponentOnRead
    {
        public Session Session;
        public object Message;
    }
    
    [ComponentOf(typeof(Scene))]
    public class NetServerComponent: Entity, IAwake<string>, IDestroy
    {
        public int ServiceId;
    }
}
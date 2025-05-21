

using System.Collections.Generic;

namespace ET
{
    public struct NetClientComponentOnRead
    {
        public Session Session;
        public object Message;
    }
    
    [ComponentOf(typeof(Scene))]
    public class NetClientComponent: Entity, IAwake<string>, IDestroy
    {
        public readonly Queue<byte[]> Sendbuffer = new ();
        public readonly Dictionary<int, RpcInfo> RequestCallbacks = new Dictionary<int, RpcInfo>();
        
        public WChannel WChannel;

        public Session Session;
    }
}
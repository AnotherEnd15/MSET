using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class DisconnectRecord : Entity,IAwake
    {
        public Queue<long> Record = new();
    }
}
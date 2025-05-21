using System;
using MemoryPack;

namespace ET.BehaviorTree
{
    [Serializable]
    public partial class DelayNode : BaseNode
    {
        public long WaitTime;
    }
}
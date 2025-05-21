using System;
using System.Collections.Generic;

namespace ET.BehaviorTree
{
    [Serializable]
    public partial class RootNode : CompNode
    {
        public RootNode(): base()
        {
            this.NodeType = NodeType.Root;
        }
    }
}
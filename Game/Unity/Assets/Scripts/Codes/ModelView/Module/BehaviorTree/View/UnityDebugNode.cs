using System;
using UnityEngine;

namespace ET.BehaviorTree.View
{
    [Serializable]
    public partial class UnityDebugNode : BaseNode
    {
        public string LogMsg;
        public AudioClip AudioClip;
    }
}
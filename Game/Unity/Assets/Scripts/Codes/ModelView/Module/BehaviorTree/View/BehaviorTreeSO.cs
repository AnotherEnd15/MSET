using System;
using UnityEngine;

namespace ET.BehaviorTree
{
    [CreateAssetMenu(menuName = "战斗/行为树")]
    public class BehaviorTreeSO : ScriptableObject
    {
        public RootNode RootNode = new();
    }
}
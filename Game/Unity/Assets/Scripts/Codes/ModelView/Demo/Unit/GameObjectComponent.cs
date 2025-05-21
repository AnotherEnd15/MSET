using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class GameObjectComponent: Entity, IAwake, IDestroy
    {
        public GameObject GameObject { get; set; }
        
        public Dictionary<string, GameObject> CutScenes = new();

        public Dictionary<long, List<GameObject>> BuffAttachObjs = new();
        public Dictionary<long, List<GameObject>> SpellCreateObjs = new();

        public GameObject StunEffect { get; set; } // 眩晕特效
    }
}
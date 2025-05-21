using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class SceneViewComponent : Entity,IAwake,IDestroy
    {
        public GameObject Map;

        // 临时的可回收go 当前场景销毁时 直接回收
        public HashSet<GameObject> TempGameObjects = new();
    }
}
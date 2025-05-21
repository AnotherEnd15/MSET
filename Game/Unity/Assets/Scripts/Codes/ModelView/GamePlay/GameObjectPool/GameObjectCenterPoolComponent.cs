using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class GameObjectCenterPoolComponent : Entity,IAwake,IDestroy
    {
        [StaticField]
        public static GameObjectCenterPoolComponent Instance;

        public Dictionary<int, GameObject> InPoolObjs = new();
        public Dictionary<int, long> ExpireTimes = new();

        // key是加载路径
        public Dictionary<string, Queue<int>> PathPool = new();

        public Dictionary<string, Queue<int>> PathUIModulePool = new();
        public Dictionary<GameObject, Queue<int>> PrefabhUIModulePool = new();

        public Dictionary<GameObject, Queue<int>> PrefabPool = new();

        public GameObject PoolParent;

        public long CheckExpireTimer;


    }
}
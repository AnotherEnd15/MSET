using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ET.GameObjectPool
{
    public static class GameObjectCenterPoolComponentSystem
    {
        public class AwakeSystem : AwakeSystem<GameObjectCenterPoolComponent>
        {
            protected override void Awake(GameObjectCenterPoolComponent self)
            {
                GameObjectCenterPoolComponent.Instance = self;
                Log.Debug($"创建GameObjectPool {self.DomainScene().SceneType}");
                self.PoolParent = new GameObject("GameObjectPool");
                self.PoolParent.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(self.PoolParent);

                //todo: 发布前改到30秒
                self.CheckExpireTimer =
                    TimerComponent.Instance.NewRepeatedTimer(10000, TimerInvokeType.GameObjectPoolExpire, self);
            }
        }

        [Invoke(TimerInvokeType.GameObjectPoolExpire)]
        public class CheckTimer : ATimer<GameObjectCenterPoolComponent>
        {
            protected override void Run(GameObjectCenterPoolComponent t)
            {
                var now = TimeHelper.ServerNow();
                using var deleteSet = HashSetComponent<int>.Create();
                foreach (var v in t.InPoolObjs)
                {
                    if (!t.ExpireTimes.TryGetValue(v.Key, out var expireTime))
                    {
                        continue;
                    }

                    if (expireTime > now)
                    {
                        continue;
                    }

                    deleteSet.Add(v.Key);
                }

                foreach (var v in deleteSet)
                {
                    GameObject.Destroy(t.InPoolObjs[v]);
                    t.InPoolObjs.Remove(v);
                }
            }
        }

        [ObjectSystem]
        class PoolDestroy : DestroySystem<GameObjectCenterPoolComponent>
        {
            protected override void Destroy(GameObjectCenterPoolComponent self)
            {
                GameObjectCenterPoolComponent.Instance = null;
                GameObject.Destroy(self.PoolParent);
                TimerComponent.Instance.Remove(ref self.CheckExpireTimer);
            }
        }


        public static async ETTask<GameObject> Load(this GameObjectCenterPoolComponent self, string path, bool isSort = true)
        {
            if (!self.PathPool.TryGetValue(path, out var queue))
            {
                queue = new();
                self.PathPool[path] = queue;
            }

            GameObject go = null;

            while (queue.Count>0)
            {
                var instanceId = queue.Dequeue();
                if (self.InPoolObjs.TryGetValue(instanceId, out go))
                {
                    if (go.transform.parent != self.PoolParent.transform)
                    {
                        throw new Exception($"池中的的GameObject 已经出池 {go.name} {path}");
                    }
                    self.InPoolObjs.Remove(instanceId);
                    go.transform.SetParent(null);
                    break;
                }
            }

            if (go == null)
            {
                go = await self.DomainScene().GetComponent<ResourcesComponent>().InstantiatePrefab(path,isSort);
                go.AddComponent<GameObjectAddressMonoBehavior>().Init(path);
            }

            return go;

        }
        public static async ETTask<GameObject> LoadUIModule(this GameObjectCenterPoolComponent self, string path)
        {
            if (!self.PathUIModulePool.TryGetValue(path, out var queue))
            {
                queue = new();
                self.PathUIModulePool[path] = queue;
            }

            GameObject go = null;

            while (queue.Count > 0)
            {
                var instanceId = queue.Dequeue();
                if (self.InPoolObjs.TryGetValue(instanceId, out go))
                {
                    if (go.transform.parent != self.PoolParent.transform)
                    {
                        throw new Exception($"池中的的GameObject 已经出池 {go.name} {path}");
                    }
                    self.InPoolObjs.Remove(instanceId);
                    go.transform.SetParent(null);
                    break;
                }
            }

            if (go == null)
            {
                go = await self.DomainScene().GetComponent<ResourcesComponent>().InstantiatePrefab(path, false);
                go.AddComponent<GameObjectAddressMonoBehavior>().Init(path,true);
            }
            return go;

        }

        public static GameObject Load(this GameObjectCenterPoolComponent self, GameObject prefab)
        {
            if (!self.PrefabPool.TryGetValue(prefab, out var queue))
            {
                queue = new();
                self.PrefabPool[prefab] = queue;
            }

            GameObject go = null;

            while (queue.Count > 0)
            {
                var instanceId = queue.Dequeue();
                if (self.InPoolObjs.TryGetValue(instanceId, out go))
                {
                    if (go.transform.parent != self.PoolParent.transform)
                    {
                        throw new Exception($"池中的的GameObject 已经出池 {go.name} {prefab.name}");
                    }

                    self.InPoolObjs.Remove(instanceId);
                    go.transform.SetParent(null);
                    break;
                }
            }

            if (go == null)
            {
                go = UnityEngine.Object.Instantiate(prefab);
                if (go.GetComponent<SortingGroup>() == null)
                {
                    go.AddComponent<SortingGroup>();
                }

                go.AddComponent<GameObjectAddressMonoBehavior>().Init(prefab);
            }
            
            return go;
        }
        public static GameObject LoadUIModule(this GameObjectCenterPoolComponent self, GameObject prefab)
        {
            if (!self.PrefabhUIModulePool.TryGetValue(prefab, out var queue))
            {
                queue = new();
                self.PrefabhUIModulePool[prefab] = queue;
            }

            GameObject go = null;

            while (queue.Count > 0)
            {
                var instanceId = queue.Dequeue();
                if (self.InPoolObjs.TryGetValue(instanceId, out go))
                {
                    if (go.transform.parent != self.PoolParent.transform)
                    {
                        throw new Exception($"池中的的GameObject 已经出池 {go.name} {prefab.name}");
                    }

                    self.InPoolObjs.Remove(instanceId);
                    go.transform.SetParent(null);
                    break;
                }
            }

            if (go == null)
            {
                go = UnityEngine.Object.Instantiate(prefab);
                go.AddComponent<GameObjectAddressMonoBehavior>().Init(prefab,true);
            }
            return go;
        }

        public static void Recycle(this GameObjectCenterPoolComponent self, GameObject gameObject)
        {
            
            if (gameObject == null)
            {
                throw new Exception($"回收GameObject进入对象池出现问题:  GameObject 为 null!");
            }
            
            if (self.IsDisposed)
                return;

            var addressConfig = gameObject.GetComponent<GameObjectAddressMonoBehavior>();
            if (addressConfig == null)
            {
                throw new Exception($"回收GameObject进入对象池出现问题: {gameObject.name} 不是从对象池里拿出的");
            }

            if (gameObject.transform.parent == self.PoolParent.transform)
            {
                throw new Exception(
                    $"入池的GameObject 不能重复入池 {addressConfig.Address} {addressConfig.gameObject?.name}");
            }

            gameObject.transform.SetParent(self.PoolParent.transform);

            
            var instanceId = gameObject.GetInstanceID();
            self.ExpireTimes[instanceId] = TimeHelper.ServerNow() + TimeHelper.Minute * 5; // 1分钟后过期 todo: 发布前改到10分钟
            
            self.InPoolObjs.Add(instanceId,gameObject);
            
            if (addressConfig.Prefab != null)
            {
                if (addressConfig.IsUIModule)
                {
                    self.PrefabhUIModulePool[addressConfig.Prefab].Enqueue(instanceId);
                }
                else
                {
                    self.PrefabPool[addressConfig.Prefab].Enqueue(instanceId);
                }
            }
            else
            {
                if(addressConfig.IsUIModule)
                {
                    self.PathUIModulePool[addressConfig.Address].Enqueue(instanceId);
                }
                else
                {
                    self.PathPool[addressConfig.Address].Enqueue(instanceId);
                }
            }
        }

    }

}
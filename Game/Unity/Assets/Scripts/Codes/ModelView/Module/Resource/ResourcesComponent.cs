using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using YooAsset;

namespace ET
{
 [FriendOf(typeof(ResourcesComponent))]
    public static class ResourcesComponentSystem
    {
        [ObjectSystem]
        public class ResourcesComponentAwakeSystem: AwakeSystem<ResourcesComponent>
        {
            protected override void Awake(ResourcesComponent self)
            {

            }
        }
        
        [ObjectSystem]
        public class ResourcesComponentDestroySystem: DestroySystem<ResourcesComponent>
        {
            protected override void Destroy(ResourcesComponent self)
            {
                foreach (var v in self.Address2Handle)
                {
                    if (v.Value is AssetHandle assetOperationHandle)
                    {
                        assetOperationHandle.Release();
                    }
                }
                self.Address2Handle.Clear();
            }
        }

        public static async ETTask<T> LoadAssetAsync_Reload<T>(this ResourcesComponent self, string address) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            await ETTask.CompletedTask;
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(address);
#else
            return await self.LoadAssetAsync<T>(address);
#endif
        }

        public static async ETTask<T> LoadAssetAsync<T>(this ResourcesComponent self, string address) where T: UnityEngine.Object
        {
            using (CoroutineLock coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources,address.GetHashCode()))
            {
                if (self.Address2Handle.TryGetValue(address, out var obj))
                {
                    var assetOperationHandle = obj as AssetHandle;
                    if (assetOperationHandle == null)
                    {
                        throw new Exception($"API使用错误， 无法通过GetAsset 加载 {address} 的资源");
                    }
                    return assetOperationHandle.AssetObject as T;
                }
                
                AssetHandle handle = YooAssets.LoadAssetAsync<T>(address);
                await handle.Task;
                self.Address2Handle[address] = handle;
                return handle.AssetObject as T;
            }
        }

        // public static void UnloadAsset(this ResourcesComponent self, string address)
        // {
        //     if (!self.Address2Handle.TryGetValue(address, out var obj))
        //     {
        //         return;
        //     }
        //     obj.ReleaseInternal();
        // }

        public static async ETTask<GameObject> InstantiatePrefab(this ResourcesComponent self, string address,bool isSort)
        {
            var prefab = await self.LoadAssetAsync<GameObject>(address);
            GameObject go = UnityEngine.Object.Instantiate(prefab);
            if (isSort&&go.GetComponent<SortingGroup>() == null)
            {
                go.AddComponent<SortingGroup>();
            }
            return go;
        }
        public static async ETTask<GameObject> InstantiatePrefabNotSort(this ResourcesComponent self, string address)
        {
            var prefab = await self.LoadAssetAsync<GameObject>(address);
            GameObject go = UnityEngine.Object.Instantiate(prefab);
            return go;
        }

        public static async ETTask<SceneHandle> LoadScene(this ResourcesComponent self, string address,UnityEngine.SceneManagement.LoadSceneMode sceneMode)
        {
            using (CoroutineLock coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, address.GetHashCode()))
            {
                SceneHandle handle = YooAssets.LoadSceneAsync(address, sceneMode);
                await handle.Task;
                return handle;
            }
        }

        public static SceneHandle GetLoadMainSceneHandle(this ResourcesComponent self, string address)
        {
            SceneHandle handle = YooAssets.LoadSceneAsync(address, LoadSceneMode.Single);
            return handle;
        }
        
        
        
        public static async ETTask<List<T>> LoadAssetListByTagAsync<T>(this ResourcesComponent self, string tag) where T: UnityEngine.Object
        {
            AssetInfo[] assetInfos = YooAssets.GetAssetInfos(tag);
            List<T> objList = new List<T>();
            List<ETTask> tasks = new List<ETTask>();
            
            async ETTask LoadOne(string add)
            {
                var asset = await self.LoadAssetAsync<T>(add);
                objList.Add(asset);
            }

            foreach (var assetInfo in assetInfos)
            {
                tasks.Add(LoadOne(assetInfo.AssetPath));
            }
            await ETTaskHelper.WaitAll(tasks);
            return objList;
        }

        public static void Unload(this ResourcesComponent self)
        {
            var pkg = YooAssets.GetPackage("DefaultPackage");
            pkg.UnloadUnusedAssetsAsync();
        }

    }
    
    [ComponentOf]
    
    public class ResourcesComponent: Entity, IAwake, IDestroy
    {
        public readonly Dictionary<string, HandleBase> Address2Handle = new ();

    }
}
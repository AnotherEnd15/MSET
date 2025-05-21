using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET
{

    public static partial class YooAssetsHelper
    {
        
        public static async ETTask<SceneHandle> LoadScene(string address, UnityEngine.SceneManagement.LoadSceneMode sceneMode)
        {
            SceneHandle handle = YooAssets.LoadSceneAsync(address, sceneMode);
            await handle.Task;
            return handle;
        }
        
        public static void Unload()
        {
            var pkg = YooAssets.GetPackage("DefaultPackage");
            pkg.UnloadUnusedAssetsAsync();
        }
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">Unity资源类型</typeparam>
        /// <param name="path">资源路径</param>
        public static ETTask<AssetHandle> LoadAssetAsync<T>(string path) where T : UnityEngine.Object
        {
            var task = ETTask<AssetHandle>.Create();

            AssetHandle op = YooAssets.LoadAssetAsync<T>(path);
            op.Completed += (AssetHandle handle) =>
            {
                task.SetResult(handle);
            };

            return task;
        }
    }
}
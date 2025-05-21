using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET
{
    public class YooAssetBehaviour : MonoBehaviour
    {
        //public AssetOperationHandle handle=null;
        public AssetHandle handle = null;


        public static async ETTask<GameObject> CreatGameObject(string path)
        {

            //var handle = YooAssetHelper.LoadAsset<GameObject>(path);
            var handle = YooAssets.LoadAssetAsync<GameObject>(path);
            await handle.Task;
            GameObject prefab = handle.AssetObject as GameObject;
            if (prefab == null)
            {
                return null;
            }
            GameObject effObj = UnityEngine.Object.Instantiate(prefab);
            effObj.AddComponent<YooAssetBehaviour>();
            effObj.GetComponent<YooAssetBehaviour>().handle = handle;
            return effObj;
        }
        private void OnDestroy()
        {
            if (handle != null && handle.IsValid)
            {
                handle.Release();
                handle = null;
            }
        }
    }
}

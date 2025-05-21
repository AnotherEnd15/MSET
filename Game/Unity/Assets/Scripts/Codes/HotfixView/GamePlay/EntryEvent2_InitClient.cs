using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using YooAsset;

namespace ET
{
    [Event]
    public class EntryEvent2_InitClient: AEvent<Scene,ET.EventType.EntryEvent2>
    {
        protected override async ETTask Run(Scene rootScene, ET.EventType.EntryEvent2 args)
        {
            Scene clientScene = SceneFactory.CreateClientScene(1, "Game");

            Root.Instance.Scene.AddComponent<ResourcesComponent>();
            Root.Instance.Scene.AddComponent<GameObjectCenterPoolComponent>();
            
#if UNITY_EDITOR
            // 为了看初始UI 这里延迟一下
            await TimerComponent.Instance.WaitAsync(1000);
#endif
            
            Log.ImportantInfo($"1. 启动流程一共耗时: {TimeHelper.ClientNow() - Init.Instance.ClientStartTime}");
            
            
            // 新的LoginUI替换旧的
            GameObject.Destroy(Init.Instance.StartUI.gameObject);
            Init.Instance.StartUI = null;
            
            await YooAssetsHelper.Init();
            
            await CustomLoading();
            
            await EventSystem.Instance.PublishAsync(clientScene, new EventType.AppStartInitFinish());
        }

        async ETTask CustomLoading()
        {
            List<ETTask> tasks = new List<ETTask>();
            tasks.Add(LoadShader());
            tasks.Add(LoadConfig());
            await ETTaskHelper.WaitAll(tasks);
        }
        
        

        async ETTask LoadConfig()
        {
            
            ConfigResult result = null;
#if UNITY_EDITOR
            EditorTool.Combine();
#endif
            var resCom = Root.Instance.Scene.GetComponent<ResourcesComponent>();
            var loadPath = "Assets/Bundles/CombinedConfigData.bytes";
            var ret = await resCom.LoadAssetAsync<TextAsset>(loadPath);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            result = MemoryPackHelper.Deserialize(typeof(ConfigResult), ret.bytes, 0, ret.bytes.Length) as ConfigResult;
            Game.AddSingleton<ConfigComponent>().LoadAll(result);
            stopwatch.Stop();
            Log.ImportantInfo($"全部配置反序列化耗时 {stopwatch.ElapsedMilliseconds}");
            #if UNITY_EDITOR
            // 发布后不需要处理
            ConfigComponent.Instance.RunConfigCheck();
            #endif
        }

        async ETTask LoadShader()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            // 加载shader
            var shaderVarHandle =
                YooAssets.LoadAssetAsync<ShaderVariantCollection>("Assets/Bundles/ShaderVariants.shadervariants");
            await shaderVarHandle.Task;
            var col = shaderVarHandle.AssetObject as ShaderVariantCollection;
            col.WarmUp();
            
            stopwatch.Stop();
            Log.ImportantInfo($"Shader加载耗时 {stopwatch.ElapsedMilliseconds}");
        }
    }
}
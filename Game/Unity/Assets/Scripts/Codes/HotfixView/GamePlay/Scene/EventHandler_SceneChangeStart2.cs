

using UnityEngine;
using UnityEngine.SceneManagement;

namespace ET
{
    [Event]
    public class EventHandler_SceneChangeStart2: AEvent<Scene,EventType.SceneChangeStart2>
    {
        protected override async ETTask Run(Scene currentScene, EventType.SceneChangeStart2 args)
        {
            currentScene.AddComponent<UIComponent>();
            var resCom = currentScene.AddComponent<ResourcesComponent>();

            currentScene.IsReady = true;
            
            // var map = await resCom.InstantiatePrefab(args.LevelConfig.AssetId_Ref.MapAsset,true );
            //
            // if (currentScene.IsDisposed)
            //     return;
            //
            // currentScene.AddComponent<SceneViewComponent>().Map = map;
            //
            // Log.Debug($"客户端地图加载完成");
            // await ETTask.CompletedTask;
        }
    }
}
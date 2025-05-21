using ET.EventType;
using UnityEngine.SceneManagement;

namespace ET
{
    [Event]
    public class EventHandler_SceneChangeStart1 : AEvent<Scene,EventType.SceneChangeStart1>
    {
        protected override async ETTask Run(Scene clientScene, SceneChangeStart1 args)
        {
            var levelConfig = args.LevelConfig;

            await UseLoadingUI(clientScene,levelConfig);
        }
        
        async ETTask UseLoadingUI(Scene clientScene, LevelConfig levelConfig)
        {
            // 先创建LoadingUI
            //var ui = (await clientScene.GetComponent<UIComponent>().Create(UIType.UI_Loading,UILayerType.Top));
            //var loadingUI = ui.GetComponent<UI_LoadingViewComponent>().UI_Loading;
            //loadingUI.Loading.value = 0;

          
            await SceneViewHelper.WaitChange2LoadingScene(clientScene, levelConfig);

            // 创建UI_Game 这个界面常驻
            await clientScene.GetComponent<UIComponent>().Create("loading");
            
            // var resCom = clientScene.GetComponent<ResourcesComponent>();
            // var loadingBar = ui.GetComponent<UI_LoginViewComponent>().UI_Login.LoadingProgress;
            // var baseValue = 96;
            // var progress = 100 - baseValue;
            // loadingBar.value = baseValue;
            // var handle = resCom.GetLoadMainSceneHandle(levelConfig.AssetId_Ref.SceneAsset);
            // while (!handle.IsDone)
            // {
            //     loadingBar.value = baseValue + handle.Progress * progress;
            //     await TimerComponent.Instance.WaitFrameAsync();
            // }

            // loadingBar.value = 100;
            // // 切换到map场景
            // handle.ActivateScene();
        }
    }
}
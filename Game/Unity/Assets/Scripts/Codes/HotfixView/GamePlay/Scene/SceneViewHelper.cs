namespace ET
{
    public static class SceneViewHelper
    {
        public static async ETTask WaitChange2LoadingScene(Scene clientScene,LevelConfig levelConfig)
        {
            // var resCom = clientScene.GetComponent<ResourcesComponent>();
            // // 先切换到loadingScene
            // var loadingScene = "Assets/LevelScenes/Loading.unity";
            // var handle = resCom.GetLoadMainSceneHandle(levelConfig.AssetId_Ref.SceneAsset);
            // await handle.Task;
            // handle.ActivateScene();
        }
    }
}
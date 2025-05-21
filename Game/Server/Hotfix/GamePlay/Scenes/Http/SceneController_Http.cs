namespace ET
{
    [SceneController(SceneType.HTTP)]
    public class SceneController_Http : ISceneController
    {
        public void OnCreate(Scene scene, StartSceneConfig startSceneConfig)
        {
            // 最后实际走代理转发 所以这里用内网ip 外网端口
            scene.AddComponent<HttpComponent,string>($"http://*:{startSceneConfig.OuterPort}{HttpConst.PATH_Root}/");
        }

        public async ETTask OnInit(Scene scene)
        {
            await ETTask.CompletedTask;
        }

        public async ETTask OnSave(Scene scene)
        {
            await ETTask.CompletedTask;
        }
    }
}
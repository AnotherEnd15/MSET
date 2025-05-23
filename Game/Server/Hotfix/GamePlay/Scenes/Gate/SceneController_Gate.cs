namespace ET
{
    [SceneController(SceneType.Gate)]
    public class SceneController_Gate : ISceneController
    {
        public void OnCreate(Scene scene,StartSceneConfig startSceneConfig)
        {
            scene.AddComponent<NetServerComponent, string>($"0.0.0.0:{startSceneConfig.OuterPort}");
            scene.AddComponent<GatePlayerComponent>();
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
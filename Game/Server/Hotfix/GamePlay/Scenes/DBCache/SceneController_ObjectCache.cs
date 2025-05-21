namespace ET
{
    [SceneController(SceneType.DBCache)]
    public class SceneController_ObjectCache : ISceneController
    {
        public void OnCreate(Scene scene, StartSceneConfig startSceneConfig)
        {
            scene.AddComponent<EntityCacheComponent>();
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
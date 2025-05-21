using System.Linq;
using ET.EventType;


namespace ET
{
    public static class SceneChangeHelper
    {
        // 场景切换协程
        public static async ETTask SceneChangeTo(Scene clientScene, long sceneInstanceId, int levelId)
        {
            Log.GetLogger().Information("切换关卡 {LevelConfigId}",levelId);

            var currentScene = await CreateCurrScene(clientScene,sceneInstanceId, levelId);
            if (currentScene.IsDisposed) // 可能连续切换场景
                return;
            

            // var msg = new C2M_SceneChangeCompleted() { };
            // clientScene.GateSession().Send(msg);

            // 创建当前场景的UI
            await EventSystem.Instance.PublishAsync(currentScene, new EventType.SceneChangeFinish());
        }
        

        public static async ETTask<Scene> CreateCurrScene(Scene clientScene,long sceneInstanceId, int levelId)
        {
            CurrentScenesComponent currentScenesComponent = clientScene.GetComponent<CurrentScenesComponent>();
            var lastSceneName = "";
            if (currentScenesComponent.Scene != null)
                lastSceneName = currentScenesComponent.Scene.Name;

            var levelConfig = LevelConfigCategory.Instance.Get(levelId);

            if (string.IsNullOrEmpty(lastSceneName))
            {
                EventSystem.Instance.Publish(clientScene, new EventType.FirstLogin());
            }

            await EventSystem.Instance.PublishAsync(clientScene, new EventType.SceneChangeStart1()
            {
                LastSceneName = lastSceneName,
                LevelConfig = levelConfig
            });
            
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene
            

            Scene currentScene = SceneFactory.CreateCurrentScene(sceneInstanceId, clientScene.Zone, levelConfig.Id.ToString(), currentScenesComponent);
           
            await EventSystem.Instance.PublishAsync(currentScene, new EventType.SceneChangeStart2()
            {
                LevelConfig = levelConfig
            });
            return currentScene;
        }

        public static void DisposeCurrScene(Scene clientScene)
        {
            CurrentScenesComponent currentScenesComponent = clientScene.GetComponent<CurrentScenesComponent>();
            currentScenesComponent?.Scene?.Dispose(); // 删除之前的CurrentScene
        }
    }
}
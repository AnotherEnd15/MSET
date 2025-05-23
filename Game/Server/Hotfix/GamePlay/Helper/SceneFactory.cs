using System.Net;
using System.Net.Sockets;
using ET;
using ET.GamePlay;
using ET.Helper;

namespace ET
{
    public static class SceneFactory
    {
        public static async ETTask<Scene> CreateStartScene(Entity parent, long id, long instanceId, int zone, string name, SceneType sceneType, StartSceneConfig startSceneConfig = null)
        {
            Scene scene = EntitySceneFactory.CreateScene(id, instanceId, zone, sceneType, name, parent);

            scene.AddComponent<MailBoxComponent, MailboxType>(MailboxType.UnOrderMessageDispatcher);
            
            SceneControllerComponent.Instance.OnCreate(scene,startSceneConfig);

            if (startSceneConfig != null)
            {
                string key = $"{zone}/{sceneType}/{instanceId}";
                string value = JsonHelper.ToJson(new SceneEtcdValue()
                {
                    OuterPort = startSceneConfig.OuterPort,
                    Ip = startSceneConfig.GetProcessConfig().ip
                });
                await EtcdManager.Instance.PutAsync(key, value);
            }

            return scene;
        }
        
        public static Scene CreateServerScene(Entity parent, long id, long instanceId, int zone, string name, SceneType sceneType)
        {
            Scene scene = EntitySceneFactory.CreateScene(id, instanceId, zone, sceneType, name, parent);

            scene.AddComponent<MailBoxComponent, MailboxType>(MailboxType.UnOrderMessageDispatcher);
            
            SceneControllerComponent.Instance.OnCreate(scene, null);
   
            return scene;
        }
    }
}
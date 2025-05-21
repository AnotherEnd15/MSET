using System;

namespace ET
{
    public static class CurrentScenesComponentSystem
    {
        public static Scene CurrentScene(this Scene clientScene)
        {
            if (clientScene.SceneType == SceneType.Current)
                return clientScene;
            return clientScene.GetComponent<CurrentScenesComponent>()?.Scene;
        }
        
        public static Scene CurrentScene(this Entity entity)
        {
            if (entity is Scene scene)
            {
                return scene.CurrentScene();
            }

            return entity.ClientScene().CurrentScene();
        }
    }
}
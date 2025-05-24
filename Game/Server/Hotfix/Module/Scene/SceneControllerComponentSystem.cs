using System;

namespace ET
{
    public static class SceneControllerComponentSystem
    {
        [ObjectSystem]
        public static void Awake(this SceneControllerComponent self)
        {
            SceneControllerComponent.Instance = self;
            self.LoadInternal();
        }

        [ObjectSystem]
        public static void Load(this SceneControllerComponent self)
        {
            self.LoadInternal();
        }

        private static void LoadInternal(this SceneControllerComponent self)
        {
            self.AllSceneControllers.Clear();
            foreach (var v in EventSystem.Instance.GetTypes(typeof(SceneControllerAttribute)))
            {
                if(v.IsAbstract || !v.IsClass)
                    continue;

                var controller = Activator.CreateInstance(v) as ISceneController;
                var attr = v.GetCustomAttributes(typeof (SceneControllerAttribute), false)[0] as SceneControllerAttribute;
                if (!self.AllSceneControllers.ContainsKey(attr.SceneType))
                    self.AllSceneControllers[attr.SceneType] = new();
                self.AllSceneControllers[attr.SceneType].Add(controller);
            }
        }
        
        public static void OnCreate(this SceneControllerComponent self, Scene scene,StartSceneConfig startSceneConfig)
        {
            if (self.AllSceneControllers.TryGetValue(scene.SceneType, out var list))
            {
                foreach (var obj in list)
                {
                    obj.OnCreate(scene,startSceneConfig);
                }
            }
        }

        public static async ETTask InitScene(this SceneControllerComponent self, Scene scene)
        {
            if (self.AllSceneControllers.TryGetValue(scene.SceneType, out var list))
            {
                foreach (var obj in list)
                {
                    try
                    {
                        await obj.OnInit(scene);
                    }
                    catch (Exception e)
                    {
                       Log.GetLogger().Error(e);
                    }
                }
            }
        }
        
        public static async ETTask SaveDB(this SceneControllerComponent self, Scene scene)
        {
            if (self.AllSceneControllers.TryGetValue(scene.SceneType, out var list))
            {
                foreach (var obj in list)
                {
                    try
                    {
                        await obj.OnSave(scene);
                    }
                    catch (Exception e)
                    {
                        Log.GetLogger().Error(e);
                    }
                }
            }
        }
    }
}
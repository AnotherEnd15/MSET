using ET.GameObjectPool;
using UnityEngine;

namespace ET
{
    public class SceneViewComponentDestroySystem : DestroySystem<SceneViewComponent>
    {
        protected override void Destroy(SceneViewComponent self)
        {
            GameObject.Destroy(self.Map);

            var gameObjectPool = GameObjectCenterPoolComponent.Instance;
            foreach (var v in self.TempGameObjects)
            {
                gameObjectPool.Recycle(v);
            }
            self.TempGameObjects.Clear();
        }
    }

    public static class SceneViewComponentSystem
    {
        public static async ETTask<GameObject> LoadTemp(this SceneViewComponent self, string path)
        {
            var poolCom = GameObjectCenterPoolComponent.Instance;

            var go = await poolCom.Load(path);
            if (self.IsDisposed)
            {
                poolCom.Recycle(go);
                return null;
            }

            self.TempGameObjects.Add(go);
            return go;
        }

        public static GameObject LoadTemp(this SceneViewComponent self, GameObject prefab)
        {
            var poolCom = GameObjectCenterPoolComponent.Instance;

            var go = poolCom.Load(prefab);
            self.TempGameObjects.Add(go);
            return go;
        }

        public static void RecycleTemp(this SceneViewComponent self, GameObject go)
        {
            if (self.IsDisposed)
                return;
            self.TempGameObjects.Remove(go);
            var poolCom = GameObjectCenterPoolComponent.Instance;
            poolCom.Recycle(go);
        }
    }

}
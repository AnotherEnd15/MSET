namespace ET
{
    [FriendOf(typeof(ServerSceneManagerComponent))]
    public static class ServerSceneManagerComponentSystem
    {
        [ObjectSystem]
        public static void Awake(this ServerSceneManagerComponent self)
        {
            ServerSceneManagerComponent.Instance = self;
        }

        [ObjectSystem]
        public static void Destroy(this ServerSceneManagerComponent self)
        {
            ServerSceneManagerComponent.Instance = null;
        }
        
        public static Scene Get(this ServerSceneManagerComponent self, int id)
        {
            Scene scene = self.GetChild<Scene>(id);
            return scene;
        }
        
        public static void Remove(this ServerSceneManagerComponent self, int id)
        {
            self.RemoveChild(id);
        }
    }
}
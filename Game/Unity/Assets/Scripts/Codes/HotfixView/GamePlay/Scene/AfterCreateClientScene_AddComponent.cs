namespace ET
{
    [Event]
    public class AfterCreateClientScene_AddComponent: AEvent<Scene,EventType.AfterCreateClientScene>
    {
        protected override async ETTask Run(Scene scene, EventType.AfterCreateClientScene args)
        {
            Log.Debug("AfterCreateClientScene");
            scene.AddComponent<UIEventComponent>();
            scene.AddComponent<UIComponent>();
            scene.AddComponent<ResourcesComponent>();
            await ETTask.CompletedTask;
        }
    }
}
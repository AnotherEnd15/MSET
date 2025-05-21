

namespace ET
{
    [Event]
    public class AfterCreateCurrentScene_AddComponent: AEvent<Scene,EventType.AfterCreateCurrentScene>
    {
        protected override async ETTask Run(Scene scene, EventType.AfterCreateCurrentScene args)
        {
            await ETTask.CompletedTask;
        }
    }
}
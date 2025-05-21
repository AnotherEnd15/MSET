namespace ET
{
    public static class SceneSaveDBComponentSystem
    {
        public class SceneSaveDBComponentAwakeSystem: AwakeSystem<SceneSaveDBComponent>
        {
            protected override void Awake(SceneSaveDBComponent self)
            {
                var time = 15 * TimeHelper.Minute + RandomGenerator.RandomNumber(-10000,10000);
                self.SaveTimer = TimerComponent.Instance.NewRepeatedTimer(time,TimerInvokeType.SceneSave,self);
            }
        }
        
        public class SceneSaveDBComponentDestroySystem: DestroySystem<SceneSaveDBComponent>
        {
            protected override void Destroy(SceneSaveDBComponent self)
            {
                TimerComponent.Instance.Remove(ref self.SaveTimer);
            }
        }

        [Invoke(TimerInvokeType.SceneSave)]
        public class SceneSaveTimer: ATimer<SceneSaveDBComponent>
        {
            protected override void Run(SceneSaveDBComponent t)
            {
                var scene = t.DomainScene();
                SceneControllerComponent.Instance.SaveDB(scene).Coroutine();
            }
        }
    }
}
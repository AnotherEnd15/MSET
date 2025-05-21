namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class SceneSaveDBComponent : Entity,IAwake,IDestroy
    {
        public long SaveTimer;
    }
}
namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ServerStateComponent : Entity,IAwake
    {
        public long CloseTime;
    }
}
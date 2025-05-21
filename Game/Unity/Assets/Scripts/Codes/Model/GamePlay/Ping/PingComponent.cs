namespace ET
{
    [ComponentOf(typeof(Session))]
    public class PingComponent: Entity, IAwake, IDestroy
    {
        public long Ping; //延迟值

        public C2G_Ping Request = new();
    }
}
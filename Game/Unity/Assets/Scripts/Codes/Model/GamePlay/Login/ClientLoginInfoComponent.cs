namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ClientLoginInfoComponent : Entity,IAwake
    {
        public string Account;
        public bool IsWX;
        public int TargetZone;

        public bool LoginSuccess;
    }
}
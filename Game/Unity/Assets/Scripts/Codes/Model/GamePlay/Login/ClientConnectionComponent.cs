namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ClientConnectionComponent : Entity,IAwake
    {
        public string ConnectStr;
        public string MapInfo;
        public long GateId { get; set; }
        public string OpenId { get; set; }
    }
}
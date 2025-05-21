namespace ET.Login
{
    public struct Wait_ChooseZone: IWaitType
    {
        public int Error { get; set; }
        public int Zone;
    }

    public struct GetServerListEvent
    {
        public C2R_LoginResponse Message;
    }
}
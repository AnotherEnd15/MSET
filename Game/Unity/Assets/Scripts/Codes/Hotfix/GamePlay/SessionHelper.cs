namespace ET
{
    public static class SessionHelper
    {
        public static Session GateSession(this Entity self)
        {
            return self.ClientScene()?.GetComponent<SessionComponent>()?.Session;
        }
    }
}
namespace ET.Server
{
    public static class SessionHelper
    {
        public static async ETTask Disconnect(Session session)
        {
            await TimerComponent.Instance.WaitAsync(1000);
            session?.Dispose();
        }
    }
}
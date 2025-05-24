using System;

namespace ET
{
    [Invoke(TimerInvokeType.SessionAcceptTimeout)]
    public class SessionAcceptTimeout: ATimer<SessionAcceptTimeoutComponent>
    {
        protected override void Run(SessionAcceptTimeoutComponent self)
        {
            try
            {
                Log.GetLogger().Information("Session创建后5秒内没收到第一个消息 销毁");
                self.Parent.Dispose();
            }
            catch (Exception e)
            {
                Log.GetLogger().Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    
    public static class SessionAcceptTimeoutComponentSystem
    {
        [ObjectSystem]
        public static void Awake(this SessionAcceptTimeoutComponent self)
        {
            self.Timer = TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + 5000, TimerInvokeType.SessionAcceptTimeout, self);
        }

        [ObjectSystem]
        public static void Destroy(this SessionAcceptTimeoutComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }
}
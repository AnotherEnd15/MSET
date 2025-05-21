namespace ET
{
    [UniqueId(100, 10000)]
    public static class TimerInvokeType
    {
        // 框架层100-200，逻辑层的timer type从200起
        public const int WaitTimer = 100;
        public const int SessionIdleChecker = 101;
        public const int ActorLocationSenderChecker = 102;
        public const int ActorMessageSenderChecker = 103;
        public const int SessionAcceptTimeout = 104;
        public const int StatisticDailyRefresh = 105;
        public const int SceneSave = 106;
        public const int EtcdHealth = 107;

    }
}
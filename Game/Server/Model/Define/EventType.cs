namespace ET.EventType
{
    public struct OnConditionGroupComplete
    {
        public int GroupConfigId;
    }

    public struct OnConditionGroupProgressChange
    {
        public int GroupConfigId;
    }

    public struct EntryEvent2
    {

    }

    public struct FirstLogin
    {

    }

    public struct SceneChangeStart1
    {
        public string LastSceneName;
        public LevelConfig LevelConfig;
    }

    public struct SceneChangeStart2
    {
        public LevelConfig LevelConfig;
    }

    public struct SceneChangeFinish
    {
    }


    public struct AppStartInitFinish
    {
    }

    public struct UnitAdd
    {
    }

    public struct UnitRemove
    {

    }

    public struct EnterMapFinish
    {
    }

    public struct LoginFinish
    {
    }

    public struct ReLoginRealm
    {

    }

    public struct AfterCreateClientScene
    {
    }

    public struct AfterCreateCurrentScene
    {
    }

    public struct OnUnitStatisticUpdate
    {
        public ConditionType statisticType;
        public int Progress;
    }
    public struct DailyRefresh
    {//每日刷新

    }

    //public struct OnPaymentFinish
    //{
    //    public string OrderId;
    //    public string ProductId;
    //}

    public struct AfterCurrencyChange
    {
        public CurrencyType CurrencyType;
        public int ChangeCount;
        public int oldNum;
        public int newNum;
        public string reason;

        public bool isAdd;
    }
    public struct AfterItemChange
    {
        public int ItemConfigID;
        public int ChangeCount;
        public int newCount;
        public string reason;

        public bool isAdd;
    }
}
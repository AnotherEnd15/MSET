namespace ET.EventType
{
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
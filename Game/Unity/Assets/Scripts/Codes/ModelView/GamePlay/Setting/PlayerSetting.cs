using MemoryPack;

namespace ET
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class PlayerSetting:StorageSingleton<PlayerSetting>
    {
        [MemoryPackOrder(0)]
        public long DailyRefreshTime { get; set; }
        
        [MemoryPackOrder(1)]
        public LanguageType LanguageType { get; set; }//语言类型
        [MemoryPackOrder(2)]
        public bool HideAccelerationByGemTips { get; set; }//隐藏宝石加速提示，每日刷新
        [MemoryPackOrder(3)]
        public bool AutoDeleteWhenReplaceEquip { get; set; }//替换后自动售出
        [MemoryPackOrder(4)]
        public bool BGMCloseState { get; set; }//背景音效
        [MemoryPackOrder(5)]
        public bool AudioEffCloseState { get; set; }//特效音效
        [MemoryPackOrder(6)]
        public bool GemSummonSkill { get; set; }//隐藏宝石召唤技能，每日刷新
        [MemoryPackOrder(7)]
        public bool GemSummonPet { get; set; }//隐藏宝石召唤宠物，每日刷新
        [MemoryPackOrder(8)]
        public bool GodNoTips { get; set; }//隐藏神像高级词条提示，每日刷新

        [MemoryPackOrder(9)]
        public bool ShowNoticeOnLogin { get; set; }//今日是否已经提示过公告了，每日刷新

        [MemoryPackOrder(10)]
        public bool ReplaceDestroyEquip { get; set; }//替换后自动销毁提示，每日刷新

        //[MemoryPackOrder(11)]
        //public bool AutoOpenBoxSetDataUpScoreStopOpen { get; set; }//自动开宝箱的装备战斗力提升时停止开启状态：false关闭，true开启
        //
        //[MemoryPackOrder(12)]
        //public bool AutoOpenBoxSetDataUpScoreStopIsOr { get; set; }//自动开宝箱的装备战斗力提升时停止和属性筛选条件满足关系：false且，true或
        //
        //[MemoryPackOrder(13)]
        //public bool AutoOpenBoxSetDataPvpKeyFullStop { get; set; }//自动开宝箱的竞技场钥匙满时停止
        //上面注释的没用了，新加的要从14开始

        [MemoryPackOrder(14)]
        public int MainFailUIShowNum { get; set; }//主线失败界面出现次数，每日刷新


        protected override void AfterLoad()
        {
            if (TimeHelper.IsDailyRefresh(DailyRefreshTime))
            {
                DailyRefreshTime = TimeHelper.ServerNow();
                HideAccelerationByGemTips = false;
                GemSummonSkill = false;
                GemSummonPet = false;
                GodNoTips = false;
                ShowNoticeOnLogin = false;
                ReplaceDestroyEquip = false;
                MainFailUIShowNum=0;
            }
        }
    }
}
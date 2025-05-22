namespace ET
{
    [UniqueId]
    public static class StateMachineType
    {
        public const int PVE_Ready = 1;
        public const int PVE_SearchMonster = 2; // 地图上前往下一个怪的阶段
    
        public const int PVE_InBattle = 4; // 战斗阶段   

        public const int PVE_DailyDungeonLimitedTimeReady = 5; // 日常限时副本的Ready状态
        public const int PVE_TreasureBoxReady = 6; // 宝箱副本的ready

        public const int PVP_Ready = 11;
     
        public const int PVP_InBattle = 14; // 战斗阶段   


        public const int Box_Idle = 101; // 宝箱_闲置状态  
        public const int Box_WaitOpenData = 102; // 宝箱_等待开启数据 
        public const int Box_OpenAnim = 103; // 宝箱_开启动画 
        public const int Box_HaveEquip = 104; // 宝箱_有装备等待处理状态
    }
}
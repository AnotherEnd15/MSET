namespace ET
{
    // 手动和服务器保持一致
    public enum ServerState
    {
        /// <summary>
        /// 流畅
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 繁忙
        /// </summary>
        HighUsage = 1,
        /// <summary>
        /// 维护
        /// </summary>
        Maintenance = 2,
    }
}
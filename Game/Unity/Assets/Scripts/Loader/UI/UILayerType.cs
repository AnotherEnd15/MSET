namespace ET
{
    public enum UILayerType
    {
        MainView = -3, // 渲染场景专用
        MainViewAdd = -2, // 场景渲染层级上额外的东西
        BattleShow = -1, // 战斗展示 
        Low = 0, // 最低的UI 所有UI一般都是这个
        Pop, // 弹出来的东西
        High, // 如剧情/教程
        Top, // 加载进度条 一些提示


        Root,//Fgui根节点
    }
}
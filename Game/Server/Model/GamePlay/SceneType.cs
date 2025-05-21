namespace ET
{
	public enum SceneType
	{
		None = -1,
		Process = 0, // 进程的root
		Router = 1, // 路由服 暂时不用
		RouterManager = 2,

        Gate = 10, // 网关, 不做任何逻辑 纯维护网络状态/转发数据
        HTTP = 11, // 监听外部发过来的http请求用
        Game = 12, // 承载玩家所有主要的逻辑(单人, 多人交互由异步逻辑处理)

        BattleManager = 20, // 管理Battle服
		Battle = 21, // 承载战斗/场景之类的逻辑
		
		GM = 30, // 专门给GM处理的服
		DBCache = 31, // 和mongodb交互的中间层, 同时也负责缓存处理
	}
}
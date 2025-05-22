// 本文件自动生成,不要手动修改
namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_Success = 0;
        // 1-11004 是SocketError请看SocketError定义
        //-----------------------------------
        // 100000-109999是Core层的错误
        
        // 110000以下的错误请看ErrorCore.cs
        
        // 这里配置逻辑层的错误码
        // 110000 - 200000是抛异常的错误
        // 200001以上不抛异常

		public const int ERR_NeedLoginRealm = 200001 ; // 需要登录Realm
		public const int ERR_RepeatLoginGate = 200100 ; // 重复登录
		public const int LoginFailedByRedisError = 200101 ; // 服务器异常200101
		public const int LoginFailedByDataError = 200102 ; // 服务器异常200102
		public const int LoginFailedByDisconnected = 200103 ; // 登录状态失效
		public const int LoginFailedByOpenIdIsNull = 200104 ; // 出现未知异常导致登录失败,请重试
		public const int ERR_ServerClosed = 200105 ; // 服务器停服维护中
		public const int ERR_BannedAccount = 200106 ; // 您的账号已被封禁,解封时间{0}
		public const int ERR_ServerNotOpened = 200107 ; // 服务器尚未对外开放
		public const int ERR_AccountDataError = 200108 ; // 账号数据出现异常,暂时无法登录,请联系客服处理
		public const int ERR_GameVersionTooOld = 200109 ; // 旧版本的客户端无法登录,稍后重新打开游戏即可自动更新

    }
}
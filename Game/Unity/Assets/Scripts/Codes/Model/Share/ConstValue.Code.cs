using FixMath;

namespace ET
{
    // 由程序使用的常量 不会是策划修改的
    
    public static partial class ConstValue
    {
        public const int SessionTimeoutTime = 4500 ; // session断开的超时时间 需要比较灵敏

        public const long PlayerDisconnectTime = 5 * TimeHelper.Minute; // 玩家掉线到自动下线的时间
    }
}
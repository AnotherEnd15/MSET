using System;

namespace ET
{
    public class TimeInfo: Singleton<TimeInfo>, ISingletonUpdate
    {
        private DateTimeOffset dt;
        
        public long ServerMinusClientTime { get; set; }

        public long FrameTime;
        
        // 服务器的时区就是utc0 这是考虑到国际服之类的情况
        public const int TimeZone = 0;
        
        public TimeInfo()
        {
            this.dt = DateTimeOffset.UnixEpoch;
            this.FrameTime = this.ClientNow();
        }

        public void Update()
        {
            this.FrameTime = this.ClientNow();
        }
        
        /// <summary> 
        /// 根据时间戳获取时间 
        /// </summary>  
        public DateTimeOffset ToDateTime(long timeStamp)
        {
            return dt.AddTicks(timeStamp * 10000);
        }
        
        // 线程安全
        public long ClientNow()
        {
            return (DateTimeOffset.UtcNow.Ticks - this.dt.Ticks) / 10000;
        }
        
        public long ServerNow()
        {
            return ClientNow() + Instance.ServerMinusClientTime;
        }
        
        public long ClientFrameTime()
        {
            return this.FrameTime;
        }
        
        public long ServerFrameTime()
        {
            return this.FrameTime + Instance.ServerMinusClientTime;
        }
        
        public long Transition(DateTimeOffset d)
        {
            return (d.Ticks - dt.Ticks) / 10000;
        }
    }
}
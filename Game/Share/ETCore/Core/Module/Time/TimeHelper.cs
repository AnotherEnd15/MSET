using System;

namespace ET
{
    public static partial class TimeHelper
    {
        public const long OneDay = 86400000;
        public const long Hour = 3600000;
        public const long Minute = 60000;
        
        [StaticField]
        public static long AddZone = TimeInfo.TimeZone * Hour;
        /// <summary>
        /// 客户端时间
        /// </summary>
        /// <returns></returns>
        public static long ClientNow()
        {
            return TimeInfo.Instance.ClientNow();
        }

        public static long ClientNowSeconds()
        {
            return ClientNow() / 1000;
        }

        public static long ToServerTime(DateTimeOffset dateTime)
        {
            return TimeInfo.Instance.Transition(dateTime);
        }

        public static DateTimeOffset ToDateTime(long servernow)
        {
            return TimeInfo.Instance.ToDateTime(servernow);
        }

        public static DateTimeOffset DateTimeNow()
        {
            return DateTimeOffset.UtcNow;
        }

        public static long ServerNow()
        {
            return TimeInfo.Instance.ServerNow();
        }
        
        public static long ClientFrameTime()
        {
            return TimeInfo.Instance.ClientFrameTime();
        }
        
        public static long ServerFrameTime()
        {
            return TimeInfo.Instance.ServerFrameTime();
        }

        /// <summary>
        /// 现在是否要刷新，针对每日固定时间点刷新的
        /// </summary>
        /// <param name="lasttime">上一次刷新的时间,单位：ms</param>
        /// <returns></returns>
        public static bool IsDailyRefresh(long lasttime)
        {
            if (lasttime == 0)
                return true;

            long nowTime = ServerNow();
            if (lasttime < nowTime) //传的时间比现在时间小
            {
                long refreshTime = GetDailyRefreshTime(lasttime);
                return refreshTime <= nowTime;
            }
            else
            {
                return false;
            }
        }

        //根据传入时间戳获取每日的零点时间
        public static long GetDailyZeroTime(long time)
        {
            DateTimeOffset dateTime = TimeInfo.Instance.ToDateTime(time + AddZone);
            long zeroTime = TimeInfo.Instance.Transition(dateTime.Date);
            return zeroTime - AddZone;
        }

        /// <summary>
        /// 获取每日刷新的刷新时间，针对每日固定时间点刷新的,传回的时间单位也是毫秒
        /// </summary>
        /// <param name="lasttime">上一次刷新的时间,单位：毫秒</param>
        /// <returns></returns>
        public static long GetDailyRefreshTime(long lasttime)
        {
 
            DateTimeOffset lastDateTime = TimeInfo.Instance.ToDateTime(lasttime+AddZone);

            long lastLocalZeroTime = TimeInfo.Instance.Transition(lastDateTime.Date);// 上次刷新的时间0点

            long refreshTime = lastLocalZeroTime + TimeHelper.OneDay-AddZone;

            //DateTimeOffset refreshdate = TimeInfo.Instance.ToDateTime(refreshTime);
            //Log.GetLogger().Information("日刷新时间="+ refreshdate.ToLocalTime().ToString());
 

            return refreshTime;
        }


        /// <summary>
        /// 现在是否要刷新，针对每周固定时间点刷新的
        /// </summary>
        /// <param name="lasttime">上一次刷新的时间,单位：毫秒</param>
        /// <returns></returns>
        public static bool IsWeekRefresh(long lasttime)
        {
            if (lasttime == 0)
                return true;
            long nowTime = ServerNow();
            if (lasttime < nowTime)//传的时间比现在时间小
            {
                long refreshTime = GetWeekRefreshTime(lasttime);
                if (refreshTime <= nowTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 获取每周刷新的刷新时间，针对每周固定时间点刷新的,传回的时间单位也是毫秒
        /// </summary>
        /// <param name="lasttime">上一次刷新的时间,单位：毫秒</param>
        /// <returns></returns>
        public static long GetWeekRefreshTime(long lasttime)
        {
            DateTimeOffset lastDateTime = TimeInfo.Instance.ToDateTime(lasttime+AddZone);
            long refreshTime;
            int week = Convert.ToInt32(lastDateTime.DayOfWeek);//星期天是0 
            week = week == 0 ? 7 : week;
            long lastLocalZeroTime = TimeInfo.Instance.Transition(lastDateTime.Date. AddDays(1-week)) ;//上次刷新的时间0点
 
            refreshTime = lastLocalZeroTime+TimeHelper.OneDay*7-AddZone;
            //DateTimeOffset refreshdate = TimeInfo.Instance.ToDateTime(refreshTime);
            //Log.GetLogger().Information("周刷新时间="+ refreshdate.ToLocalTime().ToString());


            return refreshTime;

            
        }

        /// <summary>
        /// 现在是否要刷新，针对每月固定时间点刷新的
        /// </summary>
        /// <param name="lasttime">上一次刷新的时间,单位：毫秒</param>
        /// <returns></returns>
        public static bool IsMonthRefresh(long lasttime)
        {
            if (lasttime == 0)
                return true;
            long nowTime = ServerNow();
            if (lasttime < nowTime)//传的时间比现在时间小
            {
                long refreshTime = GetMonthRefreshTime(lasttime);
                if (refreshTime <= nowTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 获取每月刷新的刷新时间，针对每月固定时间点刷新的,传回的时间单位也是毫秒
        /// </summary>
        /// <param name="lasttime">上一次刷新的时间,单位：毫秒</param>
        /// <returns>毫秒</returns>
        public static long GetMonthRefreshTime(long lasttime)
        {
            DateTimeOffset lastDateTime = TimeInfo.Instance.ToDateTime(lasttime + AddZone);
            int days = lastDateTime.Day;
            long refreshTime = TimeInfo.Instance.Transition(lastDateTime.AddDays(-days+1).Date.AddMonths(1)) -AddZone;//上次刷新的时间0点

            DateTimeOffset refreshdate = TimeInfo.Instance.ToDateTime(refreshTime);
            Log.GetLogger().Debug("月刷新时间=" + refreshdate.ToLocalTime().ToString());
            //DateTime endDay = lastDateTime.AddDays(1 - lastDateTime.Day).Date.AddMonths(1).AddSeconds(-1); //月末最后一天最后一毫秒
            //refreshTime = TimeInfo.Instance.Transition(endDay);
            return refreshTime;
        }
        /// <summary>
        /// 根据开始时间以及持续天数，获取结束时间，结束时间为当天的23:59:59
        /// </summary>
        /// <param name="startTime">开始时间</param>    
        /// <param name="days">持续天数</param>
        /// <returns>结束时间</returns>
        public static long GetEndTime(long startTime, int days)
        {
            long firstZeroTime = TimeHelper.GetDailyRefreshTime(startTime);

            return firstZeroTime+TimeHelper.OneDay*(days-1);

 
        }

        /// <summary>
        /// 获取到明天的剩余时间,显示用，剩余时间应该和服务器时间一致，不用ToLocalTime()
        /// </summary>
        public static long GetToTomorrowRemainderTime()
        {
            long tomorrow =  TimeHelper.GetDailyRefreshTime(ServerNow());

            return tomorrow - ServerNow();
        }
    }
}
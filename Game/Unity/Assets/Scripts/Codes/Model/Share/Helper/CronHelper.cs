using System;
using Quartz;

namespace ET
{
    public static class CronHelper
    {
        // 以东八区时间解析cron表达式
        public static CronExpression ToCronExpression(string cron)
        {
            var expre = new CronExpression(cron);
            #if !DOTNET
            expre.TimeZone = TimeZoneInfo.Local;
            #endif
            return expre;
        }

        public static bool IsCurrTimeValid(DateTimeOffset currInUtc, CronExpression start, CronExpression end)
        {
            var nextStart = start.GetNextValidTimeAfter(currInUtc);
            var nextEnd = end.GetNextValidTimeAfter(currInUtc);
            
            if (nextStart == null && nextEnd == null)
            {
                return false;
            }

            if (nextEnd == null)
            {
                throw new Exception($"非法配置 传入的{start.CronExpressionString} 和 {end.CronExpressionString} 出现了有开始无结束的情况");
            }

            // 此时nextEnd还没到 又没有下一次开始 那肯定是开启状态
            if (nextStart == null)
            {
                return true;
            }

            return nextStart > nextEnd;
        }
        
        /// <summary>
        /// 根据间隔时间计算当前有效时间范围 如果当前时间在生效范围内,返回这个范围 否则返回下一次的范围
        /// </summary>
        /// <param name="baseTimeInUtc"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static ValidTimeRange GetRangeTimeData(DateTimeOffset baseTimeInUtc,CronExpression StartCron,CronExpression EndCron, TimeSpan interval)
        {
            var nextStart = StartCron.GetNextValidTimeAfter(baseTimeInUtc);
            var nextEnd = EndCron.GetNextValidTimeAfter(baseTimeInUtc);

            if (nextStart == null || nextEnd == null)
            {
                return null;
            }

            if (nextStart > nextEnd)
            {
                return new ValidTimeRange { 
                    StartTime = nextStart.Value - interval, 
                    EndTime = nextEnd.Value 
                };
            }
            
            return new ValidTimeRange { 
                StartTime = nextStart.Value, 
                EndTime = nextEnd.Value 
            };

        }
    }
}
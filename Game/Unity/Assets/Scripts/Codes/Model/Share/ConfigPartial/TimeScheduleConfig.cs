using System;
using Quartz;

namespace ET
{
    public class ValidTimeRange
    {
        public DateTimeOffset StartTime;
        public DateTimeOffset EndTime;
    }
    public partial class TimeScheduleConfigCategory
    {
        public override void OnDeserialize()
        {
            base.OnDeserialize();
            foreach (var v in this._dataList)
            {
                v.OnDeserialize();
            }
        }
    }
    public partial class TimeScheduleConfig
    {
        public CronExpression StartCron;
        public CronExpression EndCron;

        public void OnDeserialize()
        {
            StartCron = CronHelper.ToCronExpression(this.CronStart);
            EndCron = CronHelper.ToCronExpression(this.CronEnd);
        }
        
        public bool IsCurrTimeValid(DateTimeOffset baseTimeInUtc)
        {
            return CronHelper.IsCurrTimeValid(baseTimeInUtc, StartCron, EndCron);
        }

        public ValidTimeRange GetRangeTimeData(DateTimeOffset baseTimeInUtc)
        {
            //todo: 应急做法 实际还是得获取beforeTime
            return CronHelper.GetRangeTimeData(baseTimeInUtc, StartCron, EndCron,GetTimeSpan(this.TimeInterval));
        }
        
        public static TimeSpan GetTimeSpan(TimeRefreshType refreshType)
        {
            switch (refreshType)
            {
                case TimeRefreshType.Hour:
                    return TimeSpan.FromHours(1);
                case TimeRefreshType.Day:
                    return TimeSpan.FromDays(1);
                case TimeRefreshType.Week:
                    return TimeSpan.FromDays(7);
                default:
                    throw new ArgumentOutOfRangeException(nameof (refreshType), refreshType, null);
            }
        }
    }
    
}
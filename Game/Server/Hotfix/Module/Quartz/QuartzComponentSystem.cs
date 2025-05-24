using System;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace ET.Module.Quartz
{
    public static class QuartzComponentSystem
    {

        [ObjectSystem]
        public static void Awake(this QuartzComponent self)
        {
            QuartzComponent.Instance = self;
        }

        [ObjectSystem]
        public static void Destroy(this QuartzComponent self)
        {
            self.Scheduler.Shutdown();
        }

        public static async ETTask Init(this QuartzComponent self)
        {
            LogProvider.IsDisabled = true;
            
            self.Scheduler = await SchedulerBuilder.Create()
                    .UseDefaultThreadPool(1)
                    .UseInMemoryStore()
                    .WithMisfireThreshold(TimeSpan.FromSeconds(1)) // 默认是60秒
                    .BuildScheduler();

            await self.Scheduler.Start();
        }

        public static async ETTask<long> AddCronJob(this QuartzComponent self,Entity entity, TimeScheduleType scheduleType)
        {
            var cron = TimeScheduleConfigCategory.Instance.Get(scheduleType).CronStart;
            return await self.AddCronJob(entity, cron);
        }
        
        public static async ETTask<long> AddCronJob(this QuartzComponent self,Entity entity, string cron)
        {
            var jobId = IdGenerater.Instance.GenerateInstanceId(Options.Instance.Process);
            var jobDetail = JobBuilder.Create<TimeScheduleJob>()
                    .UsingJobData("InstanceId", entity.InstanceId)
                    .UsingJobData("JobId", jobId)
                    .WithIdentity(entity.InstanceId.ToString())
                    .Build();

            var trigger = TriggerBuilder.Create()
                    .WithIdentity(entity.InstanceId.ToString())
                    .WithCronSchedule(cron)
                    .Build();

           
            self.JobKeys[jobId] = jobDetail.Key;
            await self.Scheduler.ScheduleJob(jobDetail, trigger);
            return jobId;
        }

        public static async ETTask RemoveJob(this QuartzComponent self, long id)
        {
            if (!self.JobKeys.TryGetValue(id, out var key))
            {
                return;
            }
            await self.Scheduler.DeleteJob(key);
        }
    }
}
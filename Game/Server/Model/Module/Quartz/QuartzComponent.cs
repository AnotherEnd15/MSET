using System.Collections.Generic;
using Quartz;

namespace ET.Module
{
    [ComponentOf(typeof(Scene))]
    public class QuartzComponent : Entity,IAwake,IDestroy
    {
        [StaticField]
        public static QuartzComponent Instance;

        public IScheduler Scheduler;

        public Dictionary<long, JobKey> JobKeys = new();
    }
}
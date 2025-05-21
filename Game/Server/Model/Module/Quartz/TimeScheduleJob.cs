using System.Threading.Tasks;
using Quartz;

namespace ET.Module
{
    public struct TimeScheduleEvent
    {
        public long JobId;
    }

    public class TimeScheduleJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var instanceId = (long)context.Get("InstanceId");
            var jobId = (long)context.Get("JobId");
            
            MainThreadSynchronizationContext.Instance.Post(() =>
            {
                var entity = Root.Instance.Get(instanceId);
                if (entity == null)
                    return;
                EventSystem.Instance.Publish(entity, new TimeScheduleEvent() { JobId = jobId}); 
            });

            return Task.CompletedTask;
        }
    }
}
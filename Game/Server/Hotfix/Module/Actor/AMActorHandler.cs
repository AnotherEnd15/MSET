using System;

namespace ET
{
    [EnableClass]
    public abstract class AMActorHandler<E, Message>: IMActorHandler where E : Entity where Message : class, IActorMessage
    {
        protected abstract ETTask Run(E entity, Message message);

        public async ETTask Handle(Entity entity, int fromProcess, object actorMessage)
        {
            if (actorMessage is not Message msg)
            {
                Log.GetLogger().Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Message).Name}");
                return;
            }

            if (entity is not E e)
            {
                Log.GetLogger().Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof (E).Name} --{typeof (Message).Name}");
                return;
            }

            long start = TimeHelper.ServerNow();
            await this.Run(e, msg);
            var interval = TimeHelper.ServerNow() - start;
            if (interval - start > 2000)
            {
                Log.GetLogger().Error($"Actor消息处理耗时过长: {entity.InstanceId} {actorMessage.GetType().FullName}");
            }
        }

        public Type GetRequestType()
        {
            return typeof (Message);
        }

        public Type GetResponseType()
        {
            return null;
        }
    }
}
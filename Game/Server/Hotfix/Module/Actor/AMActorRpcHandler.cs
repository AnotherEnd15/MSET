using System;

namespace ET.Server
{
    [EnableClass]
    public abstract class AMActorRpcHandler<E, Request, Response>: IMActorHandler where E : Entity where Request : class, IActorRequest where Response : class, IActorResponse
    {
        protected abstract ETTask Run(E actor, Request request, Response response);

        public async ETTask Handle(Entity entity, int fromProcess, object actorMessage)
        {
            try
            {
                if (actorMessage is not Request request)
                {
                    Log.GetLogger().Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Request).Name}");
                    return;
                }

                if (entity is not E ee)
                {
                    Log.GetLogger().Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof (E).Name} --{typeof (Request).Name}");
                    return;
                }

                int rpcId = request.RpcId;
                Response response = Activator.CreateInstance<Response>();
                
                try
                {
                    long start = TimeHelper.ServerNow();
                    await this.Run(ee, request, response);
                    var interval = TimeHelper.ServerNow() - start;
                    if (interval - start > 2000)
                    {
                        Log.GetLogger().Error($"Actor消息处理耗时过长: {entity.InstanceId} {actorMessage.GetType().FullName}");
                    }
                }
                catch (Exception exception)
                {
                    Log.GetLogger().Error(exception);
                    response.Error = ErrorCore.ERR_RpcFail;
                    response.Message = exception.ToString();
                }
                
                response.RpcId = rpcId;
                ActorHandleHelper.Reply(fromProcess, response);
            }
            catch (Exception e)
            {
                throw new Exception($"解释消息失败: {actorMessage.GetType().FullName}", e);
            }
        }

        public Type GetRequestType()
        {
            return typeof (Request);
        }

        public Type GetResponseType()
        {
            return typeof (Response);
        }
    }
}
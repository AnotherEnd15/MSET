using System;

namespace ET.Server
{
    public static class ActorHandleHelper
    {
        public static void Reply(int fromProcess, IActorResponse response)
        {
            Session replySession = NetInnerComponent.Instance.Get(fromProcess);
            replySession.Send(response);
        }
        
        public static void HandleIActorResponse(IActorResponse response)
        {
            ActorMessageSenderComponent.Instance.HandleIActorResponse(response);
        }
        
        /// <summary>
        /// 分发actor消息
        /// </summary>
        [EnableAccessEntiyChild]
        public static async ETTask HandleIActorRequest(long actorId, IActorRequest iActorRequest)
        {
            InstanceIdStruct instanceIdStruct = new(actorId);
            int fromProcess = instanceIdStruct.Process;
            instanceIdStruct.Process = Options.Instance.Process;
            long realActorId = instanceIdStruct.ToLong();

            Entity entity = Root.Instance.Get(realActorId);
            if (entity == null)
            {
                Log.GetLogger().Warning("actor id not found: {actorId} {realActorId}",actorId,realActorId);
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                Reply(fromProcess, response);
                return;
            }

            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null)
            {
                Log.GetLogger().Warning($"actor not found mailbox: {entity.GetType().Name} {realActorId} {iActorRequest}");
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                Reply(fromProcess, response);
                return;
            }
            
            switch (mailBoxComponent.MailboxType)
            {
                case MailboxType.MessageDispatcher:
                {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId, time:10000))
                    {
                        if (entity.InstanceId != realActorId)
                        {
                            Log.GetLogger().Warning("entity instanceId change {actorId} {realActorId} {newInstanceId}",actorId,realActorId,entity.InstanceId);
                            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                            Reply(fromProcess, response);
                            break;
                        }
                        await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorRequest);
                    }
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher:
                {
                    await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorRequest);
                    break;
                }
                case MailboxType.GateSession:
                default:
                    throw new Exception($"no mailboxtype: {mailBoxComponent.MailboxType} {iActorRequest}");
            }
        }
        
        /// <summary>
        /// 分发actor消息
        /// </summary>
        [EnableAccessEntiyChild]
        public static async ETTask HandleIActorMessage(long actorId, IActorMessage iActorMessage)
        {
            InstanceIdStruct instanceIdStruct = new(actorId);
            int fromProcess = instanceIdStruct.Process;
            instanceIdStruct.Process = Options.Instance.Process;
            long realActorId = instanceIdStruct.ToLong();
            
            Entity entity = Root.Instance.Get(realActorId);
            if (entity == null)
            {
                Log.GetLogger().Information($"not found actor: {realActorId} {iActorMessage.GetType().Name}");
                return;
            }
            
            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null)
            {
                Log.GetLogger().Error($"actor not found mailbox: {entity.GetType().Name} {realActorId} {iActorMessage}");
                return;
            }

            switch (mailBoxComponent.MailboxType)
            {
                case MailboxType.MessageDispatcher:
                {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId, time:10000))
                    {
                        if (entity.InstanceId != realActorId)
                        {
                            break;
                        }
                        await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorMessage);
                    }
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher:
                {
                    await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorMessage);
                    break;
                }
                case MailboxType.GateSession:
                {
                    if (entity is Session gateSession)
                    {
                        // 发送给客户端
                        gateSession.Send(iActorMessage);
                    }
                    break;
                }
                default:
                    throw new Exception($"no mailboxtype: {mailBoxComponent.MailboxType} {iActorMessage}");
            }
        }
    }
}
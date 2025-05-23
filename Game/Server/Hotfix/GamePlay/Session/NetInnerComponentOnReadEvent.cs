﻿using System;

namespace ET
{
    [Event]
    public class NetInnerComponentOnReadEvent: AEvent<Scene,NetInnerComponentOnRead>
    {
        protected override async ETTask Run(Scene scene, NetInnerComponentOnRead args)
        {
            try
            {
                long actorId = args.ActorId;
                object message = args.Message;
                
                // 收到actor消息,放入actor队列
                switch (message)
                {
                    case IActorResponse iActorResponse:
                    {
                        ActorHandleHelper.HandleIActorResponse(iActorResponse);
                        break;
                    }
                    case IActorRequest iActorRequest:
                    {
                        await ActorHandleHelper.HandleIActorRequest(actorId, iActorRequest);
                        break;
                    }
                    case IActorMessage iActorMessage:
                    {
                        await ActorHandleHelper.HandleIActorMessage(actorId, iActorMessage);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.GetLogger().Error($"InnerMessageDispatcher error: {args.Message.GetType().Name}\n{e}");
            }

            await ETTask.CompletedTask;
        }
    }
}
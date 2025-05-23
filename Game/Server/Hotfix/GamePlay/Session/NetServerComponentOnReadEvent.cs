using ET.Module;

namespace ET
{
    [Event]
    public class NetServerComponentOnReadEvent: AEvent<Scene,NetServerComponentOnRead>
    {
        protected override async ETTask Run(Scene scene, NetServerComponentOnRead args)
        {
            Session session = args.Session;
            object message = args.Message;

            if (message is IResponse response)
            {
                session.OnResponse(response);
                return;
            }

            
            if (message is IActorRequest request)
            {
                var seesionPlayerCom = session.GetComponent<SessionPlayerComponent>();
                if (seesionPlayerCom == null)
                {
                    Log.GetLogger().Information("玩家还未登录/重登 就发送了actor消息 {Type}",request.GetType().FullName);
                    return;
                }

                // todo 加入协议cd
                // long unitId = seesionPlayerCom.Player.Id;
                // var opCode = NetServices.Instance.GetOpcode(message.GetType());
                
                // if (!OpcodeHelper.ProtoCDDefine.TryGetValue(opCode, out var cd))
                // {
                //     cd = 50;
                // }
                //
                // if (cd >0 && !ActionCooldownComponent.Instance.TryAddCD(unitId, opCode, cd))
                // {
                //     IResponse iResponse = ActorHelper.CreateResponse(request, ErrorCode.ERR_TooFast);
                //     iResponse.RpcId = request.RpcId;
                //     session.Send(iResponse);
                //     return;
                // }
            }



            // 根据消息接口判断是不是Actor消息，不同的接口做不同的处理,比如需要转发给Chat Scene，可以做一个IChatMessage接口
            switch (message)
            {
                case IActorPlayerRequest actorPlayerRequest:
                {
                    var playerActorId = session.GetComponent<SessionPlayerComponent>().PlayerActorId;
                    if (playerActorId == 0)
                    {
                        return;
                    }

                    var rpcId = actorPlayerRequest.RpcId;
                    var instanceId = session.InstanceId;
                    IResponse iResponse = await MessageHelper.CallActor(playerActorId, actorPlayerRequest);
                    iResponse.RpcId = rpcId;
                    // session可能已经断开了，所以这里需要判断
                    if (session.InstanceId == instanceId)
                    {
                        session.Send(iResponse);
                    }
                    

                    break;
                }

                case IActorPlayerMessage acterPlayerMessage:
                {
                    var playerActorId = session.GetComponent<SessionPlayerComponent>().PlayerActorId;
                    if (playerActorId == 0)
                    {
                        return;
                    }
                    
                    MessageHelper.SendActor(playerActorId, acterPlayerMessage);
                    break;
                }


                case IActorRequest actorRequest:  // 分发IActorRequest消息，目前没有用到，需要的自己添加
                {
                    break;
                }
                case IActorMessage actorMessage:  // 分发IActorMessage消息，目前没有用到，需要的自己添加
                {
                    break;
                }
				
                default:
                {
                    // 非Actor消息
                    MessageDispatcherComponent.Instance.Handle(session, message);
                    break;
                }
            }
        }
    }
}
namespace ET
{
    // 不需要返回消息
    public interface IActorMessage: IMessage
    {
        
    }

    public interface IActorRequest: IRequest
    {
        
    }

    public interface IActorResponse: IResponse
    {
    }

    // 标识消息发往Game, 亦或者是Battle
    public interface IActorPlayerMessage : IActorMessage {
     
    }
    public interface IActorPlayerRequest : IActorRequest {
      
    }
    public interface IActorPlayerResponse : IActorResponse { }

    public interface IActorBattleMessage : IActorMessage {
 
    }
    public interface IActorBattleRequest : IActorRequest {
      
    }
    public interface IActorBattleResponse : IActorResponse { }
}
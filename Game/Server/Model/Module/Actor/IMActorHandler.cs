using System;

namespace ET
{
    public interface IMActorHandler
    {
        ETTask Handle(Entity entity, int fromProcess, object actorMessage);
        Type GetRequestType();
        Type GetResponseType();
    }
}
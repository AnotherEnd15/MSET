using System;
using System.IO;

namespace ET
{
    public static class ActorHelper
    {
        public static IActorResponse CreateResponse(IActorRequest iActorRequest, int error)
        {
            Type responseType = OpcodeTypeComponent.Instance.GetResponseType(iActorRequest.GetType());
            IActorResponse response = (IActorResponse)Activator.CreateInstance(responseType);
            response.Error = error;
            response.RpcId = iActorRequest.RpcId;
            return response;
        }
    }
}


using System.Collections.Generic;
using System.IO;
using ET.GamePlay;


namespace ET.Server
{
    public static partial class MessageHelper
    {
        
        public static void BroadcastToGates(int zone, IActorMessage actorMessage)
        {
            var gates = StartSceneService.Instance.ZoneScenes[(zone,SceneType.Gate)];
            foreach (var v in gates)
            {
                MessageHelper.SendActor(v.InstanceId,actorMessage);
            }
        }

        public static void SendToClient(Player player, IActorMessage message)
        {
            if (player.ClientSessionId == 0)
                return;
            ActorMessageSenderComponent.Instance.Send(player.ClientSessionId, message);
        }
        
        
        /// <summary>
        /// 发送协议给Actor
        /// </summary>
        /// <param name="actorId">注册Actor的InstanceId</param>
        /// <param name="message"></param>
        public static void SendActor(long tarActorId, IActorMessage message)
        {
            ActorMessageSenderComponent.Instance.Send(tarActorId, message);
        }
        
        /// <summary>
        /// 发送RPC协议给Actor
        /// </summary>
        /// <param name="actorId">注册Actor的InstanceId</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async ETTask<IActorResponse> CallActor(long actorId, IActorRequest message)
        {
            return await ActorMessageSenderComponent.Instance.Call(actorId, message);
        }
    }
}
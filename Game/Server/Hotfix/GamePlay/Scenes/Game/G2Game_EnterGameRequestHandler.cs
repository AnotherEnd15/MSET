using ET.Proto;
using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.GamePlay.Scenes.Game
{
    internal class G2Game_EnterGameRequestHandler : AMActorRpcHandler<Scene, G2Game_EnterGameRequest, Game2G_EnterGameResponse>
    {
        protected override async ETTask Run(Scene scene, G2Game_EnterGameRequest request, Game2G_EnterGameResponse response)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnitId, request.Uid))
            {
                // TODO 查询是否有此uid的玩家, 如果有, T掉它旧的gateSession
                // 如果没有, 就查询数据库, 数据库还没有 就创建
                // 最后绑定GateActorId
                var playerComp = scene.GetComponent<PlayerComponent>();
                if (!playerComp.TryGetChild<Player>(request.Uid, out var player))
                {
                    player = playerComp.AddChildWithId<Player>(request.Uid);
                }
                if (player.GateActorId != 0)
                {
                    MessageHelper.SendActor(player.GateActorId, new Game2Gate_CloseSession());
                }
                player.GateActorId = request.GateActorId;
                player.ClientSessionId = request.ClientSessionId;
            }
        }
    }
}

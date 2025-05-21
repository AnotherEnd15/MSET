using ET.Proto;
using ET.Server;

namespace ET.GamePlay.Scenes.Game;

[ActorMessageHandler]
public class G2Game_PlayerSessionCloseHandler : AMActorHandler<Player, G2Game_PlayerSessionClose>
{
    protected override async ETTask Run(Player entity, G2Game_PlayerSessionClose message)
    {
        if (message.SessionId != entity.ClientSessionId)
        {
            Log.GetLogger().Error("触发SessionClose的sessionId非法. {Now} -> {Msg}", entity.ClientSessionId, message.SessionId);
            return;
        }

        // todo 触发掉线倒计时销毁逻辑
        entity.ClientSessionId = 0;
        entity.GateActorId = 0;
        await ETTask.CompletedTask;
    }
}
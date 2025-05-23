using ET.Proto;
using ET;

namespace ET.Gate;

[ActorMessageHandler]
public class Game2Gate_CloseSessionHandler : AMActorHandler<GatePlayer, Game2Gate_CloseSession>
{
    protected override async ETTask Run(GatePlayer entity, Game2Gate_CloseSession message)
    {
        var session = entity.Session;
        if (session != null && !session.IsDisposed)
        {
            session.Send(new G2C_RepeatLogin());
            session.GetComponent<SessionPlayerComponent>().IsKicked = true;
            SessionHelper.Disconnect(session).Coroutine();
        }
        await ETTask.CompletedTask;
    }
}
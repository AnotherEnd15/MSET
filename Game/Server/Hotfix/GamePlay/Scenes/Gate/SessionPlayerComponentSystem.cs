using ET.Proto;
using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.GamePlay.Scenes.Gate
{
    [ObjectSystem]
    internal class SessionPlayerComponentDestroySystem : DestroySystem<SessionPlayerComponent>
    {
        protected override void Destroy(SessionPlayerComponent self)
        {
            if (self.IsKicked)
                return;
            var msg = new G2Game_PlayerSessionClose() { SessionId = self.GetParent<Session>().InstanceId };
            MessageHelper.SendActor(self.PlayerActorId, msg);
        }
    }
}

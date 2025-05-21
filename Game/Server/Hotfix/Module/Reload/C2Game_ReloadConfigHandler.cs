using ET.Proto;
using ET.Server;

namespace ET.Module
{
    [ActorMessageHandler]
    public class C2Game_ReloadConfigHandler : AMActorRpcHandler<Player, C2Game_ReloadConfigRequest ,Game2C_ReloadConfigResponse>
    {
        protected override async ETTask Run(Player unit, C2Game_ReloadConfigRequest request, Game2C_ReloadConfigResponse response)
        {
            if (Options.Instance.Develop == 0)
            {
                return;
            }

            
            ConfigComponent.Instance.LoadAll();
            Log.GetLogger().Information("重载配置表");
            
            await ETTask.CompletedTask;
        }
    }
}
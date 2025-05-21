using System;
using ET.Proto;
using ET.Server;

namespace ET;

[ActorMessageHandler]
public class QuerySavingCountHandler : AMActorRpcHandler<Scene, QuerySavingCountRequest, QuerySavingCountResponse>
{
    protected override async ETTask Run(Scene scene, QuerySavingCountRequest request, QuerySavingCountResponse response)
    {
        await ETTask.CompletedTask;
        response.Count = (int)scene.GetComponent<EntityCacheComponent>().GetSavingCount();
    }
}
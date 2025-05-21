using System;
using ET.Proto;
using ET.Server;

namespace ET;

[ActorMessageHandler]
public class DeleteEntityRequestHandler : AMActorRpcHandler<Scene, DeleteEntityRequest, DeleteEntityResponse>
{
    protected override async ETTask Run(Scene scene, DeleteEntityRequest request, DeleteEntityResponse response)
    {
        var id = request.Id;
        var collectionName = request.Collection;
        var type = EventSystem.Instance.GetType(request.Type);
        var cache = scene.GetComponent<EntityCacheComponent>().GetCache(request.Zone, type,collectionName);
        using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.EntityCache, id))
        {
            await cache.Remove(id, request.DeleteFromDB);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ET.Proto;
using ET.Server;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace ET;

[ActorMessageHandler]
public class QueryEntityRequestHandler : AMActorRpcHandler<Scene, QueryEntityRequest, QueryEntityResponse>
{
    protected override async ETTask Run(Scene scene, QueryEntityRequest request, QueryEntityResponse response)
    {
        for (int i = 0; i < request.TypeList.Count; i++)
        {
            var id = request.IdList[i];
            var collectionName = request.CollectionNames[i];
            var type = EventSystem.Instance.GetType(request.TypeList[i]);
            var cache = scene.GetComponent<EntityCacheComponent>().GetCache(request.Zone, type,collectionName);
            var currTime = TimeHelper.ServerNow();
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.EntityCache, id))
            {
                var target = await cache.Get(id);
                if (target != null)
                {
                    response.EntityList.Add(MongoHelper.Serialize(target));
                }
            }

            var delta = TimeHelper.ServerNow() - currTime;
            if (delta > 10000)
            {
                Log.GetLogger().Warning("QueryEntity {Type} {Id} 用时过久 {Time}",type.FullName,id, delta);
            }
        }
        
    }
}
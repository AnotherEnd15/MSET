using System;
using ET.Proto;
using ET.Server;
using Serilog;

namespace ET;

[ActorMessageHandler]
public class EntitySaveRequestHandler : AMActorRpcHandler<Scene, EntitySaveRequest, EntitySaveResponse>
{
    protected override async ETTask Run(Scene scene, EntitySaveRequest request, EntitySaveResponse response)
    {
        var recvTime = TimeHelper.ServerNow();
        if (recvTime - request.SendTime > 1000)
            Log.GetLogger().Warning("传输耗时 : {List} {Delta} ", request.CollectionNames.ListToString(), recvTime - request.SendTime);

        var com = scene.GetComponent<EntityCacheComponent>();
        for (int i = 0; i < request.Entitys.Count; i++)
        {
            var entity = MongoHelper.Deserialize<Entity>(request.Entitys[i]);
            var collectionName = request.CollectionNames[i];
            var cache = com.GetCache(request.Zone, entity.GetType(),collectionName);
            var startTime = TimeHelper.ServerNow();
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.EntityCache, entity.Id))
            {
                if (!await cache.CheckAndSave(entity,request.SaveImd))
                {
                    // todo
                    response.Error = ErrorCode.ERR_Success;
                    break;
                }

                var delta = TimeHelper.ServerNow() - startTime;
                if (delta >= 2000)
                {
                    Log.GetLogger().Information("保存结束 {Type} {Id} {Version} {Delta}", entity.GetType().Name, entity.Id, entity.Version, delta);
                }
            }
        }
    }
    

}
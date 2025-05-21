using System;
using System.Diagnostics;
using ET.Server;
using MongoDB.Driver;
using Serilog;

namespace ET;


[ObjectSystem]
public class EntityCacheUpdateSystem: UpdateSystem<EntityCacheComponent>
{
    protected override void Update(EntityCacheComponent self)
    {
        self.Update();
    }
}

public static class EntityCacheComponentSystem
{
    private static ILogger Logger = Log.GetLogger();

    public static int GetDBIndexQueueId(this EntityCacheComponent self, long id)
    {
        if (self.Id2DBQueueMap.TryGetValue(id, out var queueId))
            return queueId;
        self.LastCacheId++;
        queueId = self.LastCacheId;
        
        self.Id2DBQueueMap[id] = queueId;
        
        if (self.LastCacheId >= DBComponent.TaskCount)
            self.LastCacheId = -1;
        return queueId;
    }

    public static EntityCache GetCache(this EntityCacheComponent self,int zone,Type type,string CollectionName)
    {
        var key = (zone, type,CollectionName);
        if (!self.AllCaches.TryGetValue(key, out var cache))
        {
            cache = self.AddChild<EntityCache>();
            cache.Zone = zone;
            cache.CollectionName = CollectionName;
            self.AllCaches[key] = cache;
        }
        
        return cache;
    }

    public static void Add2Queue(this EntityCacheComponent self, EntityCache entityCache, long id, long version)
    {
        self.SaveOpCount++;
        self.SaveQueue.Enqueue((entityCache, id, version));
    }

    public static void Update(this EntityCacheComponent self)
    {
        if (self.SaveQueue.Count == 0)
        {
            return;
        }

        var now = TimeHelper.ServerNow();
        if (now - self.LastLogTime >= 3000)
        {
            self.LastLogTime = now;
            var savingCount = self.GetSavingCount();
            if (savingCount > 0 || self.SaveOpCount > 0)
                Logger.Information("当前实体保存数量: {SceneId} {Count} {SaveOpCount}", self.DomainScene().Id, savingCount, self.SaveOpCount);
            self.SaveOpCount = 0;
        }

        // 当前的db保存数量太多了,再继续也只能增加协程锁等待队列,所以不处理.
        if (self.DBSavingCount >= 60)
        {
            return;
        }
        
        while (self.SaveQueue.Count > 0)
        {
            var save = self.SaveQueue.Dequeue();
            if(!save.entityCache.CacheEntity.TryGetValue(save.entityId,out var node) || node.Value.Version != save.version)
                continue;
            save.entityCache.Save(node.Value).Coroutine();
        }
    }

    public static long GetSavingCount(this EntityCacheComponent self)
    {
        long total = 0;
        
        total += self.DBSavingCount;
        total += self.SaveQueue.Count;
        return total;
    }
}
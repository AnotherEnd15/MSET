using System;
using ET.Server;
using MongoDB.Driver;
using Serilog;

namespace ET;

public static class EntityCacheSystem
{
    private const int RemoveInterval = 24 * 60 * 60 * 1000;
    
    private static ILogger Logger = Log.GetLogger();

    public static async ETTask<bool> CheckAndSave(this EntityCache self, Entity target,bool saveImd)
    {
        // 检测版本号
        if (!self.CacheEntity.ContainsKey(target.Id))
        {
            await self.Get(target.Id);
        }

        if (self.CacheEntity.TryGetValue(target.Id, out var lastCache))
        {
            if (lastCache.Value.Version >= target.Version)
            {
                Logger.Error("保存时出现了一个低版本号的旧文档 {Type} {Id} {LastVersion} {TargetVersion}", target.GetType().FullName, target.Id,
                    lastCache.Value.Version, target.Version);
                return false;
            }
        }
        
        //await self.Save(target);
        
        if (saveImd)
        {
            await self.Save(target);
        }
        else
        {
            self.GetParent<EntityCacheComponent>().Add2Queue(self, target.Id, target.Version);
            self.InSavingEntity.Add(target.Id);
        }

        self.Add(target);

        return true;
    }

    public static void Add(this EntityCache self, Entity target)
    {
        var now = TimeHelper.ServerNow();
        if (self.CacheEntity.TryGetValue(target.Id, out var node))
        {
            self.RecentlyUsed.Remove(node);
            self.RecentlyUsed.AddLast(node);
            self.EntityOpTime[target.Id] = now;
            
            node.Value = target;
        }
        else
        {
            node = self.RecentlyUsed.AddLast(target);
            self.EntityOpTime[target.Id] = now;
            self.CacheEntity.Add(target.Id, node);
        }

        var first = self.RecentlyUsed.First;
        while (first != null)
        {
            var opTime = self.EntityOpTime[first.Value.Id];
            if (opTime - now < RemoveInterval)
            {
                break;
            }

            if (!self.InSavingEntity.Contains(first.Value.Id))
            {
                self.CacheEntity.Remove(first.Value.Id);
                self.EntityOpTime.Remove(first.Value.Id);
                first = first.Next;
                self.RecentlyUsed.RemoveFirst();
            }
            else
            {
                break;
            }
        }

    }

    public static async ETTask<Entity> Get(this EntityCache self, long id)
    {
        if (self.CacheEntity.TryGetValue(id, out var node))
        {
            self.RecentlyUsed.Remove(node);
            self.RecentlyUsed.AddLast(node);
            self.EntityOpTime[id] = TimeHelper.ServerNow();
            
            return node.Value;
        }

        // 从db中加载
        Entity result = null;

        var ququeId = self.DomainScene().GetComponent<EntityCacheComponent>().GetDBIndexQueueId(id);
        
        using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, ququeId))
        {
            var db = DBManagerComponent.Instance.GetZoneDB(self.Zone);
            var filter = Builders<Entity>.Filter.Eq(v => v.Id, id);
            var target = await db.GetCollection(self.CollectionName).FindAsync(filter);
            result = target.FirstOrDefault();
        }

        if (result != null)
        {
            self.Add(result);
            return result;
        }
        
        return null;
    }

    public static async ETTask Save(this EntityCache self, Entity target)
    {
        var com = self.GetParent<EntityCacheComponent>();
        com.DBSavingCount++;
        self.InSavingEntity.Remove(target.Id); // 这里就可以去掉了, 因为这个东西只用来判断还需不需要缓存. 到了这里就不需要缓存了.
        //Logger.Information("准备写入 {Type} {Id} {Version} {SavingCount}]", target.GetType().Name, target.Id, target.Version,com.DBSavingCount);
        var coroutineId =     self.DomainScene().GetComponent<EntityCacheComponent>().GetDBIndexQueueId(target.Id);;

        using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, coroutineId))
        {
            var db = DBManagerComponent.Instance.GetZoneDB(self.Zone);
            var collection = db.GetCollection(self.CollectionName);
            var filter = Builders<Entity>.Filter.Eq(v => v.Id, target.Id);
            var startTime = TimeHelper.ServerNow();
            await collection.ReplaceOneAsync(filter, target, new ReplaceOptions() { IsUpsert = true });
            var saveInterval = TimeHelper.ServerNow() - startTime;
            com.DBSavingCount--;
            if (saveInterval >= 1000)
                Logger.Information("写入完成 {Type} {Id} {Version} {CoroutineId} {SavingCount} {SaveInterval}", target.GetType().Name, target.Id,
                    target.Version, coroutineId, com.DBSavingCount, saveInterval);
        }
    }

    public static async ETTask Remove(this EntityCache self, long id,bool deleteFromDB)
    {
        if (self.CacheEntity.TryGetValue(id, out var node))
        {
            self.RecentlyUsed.Remove(node);
            self.CacheEntity.Remove(id);
        }

        if (deleteFromDB)
        {
            var ququeId = self.DomainScene().GetComponent<EntityCacheComponent>().GetDBIndexQueueId(id);
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, ququeId))
            {
                var db = DBManagerComponent.Instance.GetZoneDB(self.Zone);
                var collection = db.GetCollection(self.CollectionName);
                await collection.DeleteOneAsync(d => d.Id == id);
                Logger.Information("从数据库里删除数据 {SceneName} {Type} {Id}", self.DomainScene().Name, self.CollectionName, id);
            }
        }
    }
}
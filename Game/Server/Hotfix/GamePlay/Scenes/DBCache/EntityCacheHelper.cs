using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using ET.Proto;
using ET.Server;
using MongoDB.Driver;
using Serilog;

namespace ET;

public static class EntityCacheHelper
{
    private static ILogger Logger = Log.GetLogger();
    public static async ETTask<T> Query<T>(int zone, long id) where T:Entity
    {
        return await Query<T>(zone, id, typeof (T).Name);
    }
    
    public static async ETTask<T> Query<T>(int zone, long id,string collectionName) where T:Entity
    {
        if (string.IsNullOrEmpty(collectionName))
            collectionName = typeof(T).Name;

        var request = new QueryEntityRequest { Zone = zone, };
        request.IdList.Add(id);
        request.TypeList.Add(typeof(T).FullName);
        request.CollectionNames.Add(collectionName);

        var dbCacheList = StartSceneService.Instance.ZoneScenes[(zone, SceneType.DBCache)];
        var index = id % dbCacheList.Count;
        var sceneActorId = dbCacheList[(int)index].InstanceId;

        var resp = (QueryEntityResponse) await MessageHelper.CallActor(sceneActorId, request);
        if (resp.EntityList.Count == 0)
            return null;

        return MongoHelper.Deserialize<T>(resp.EntityList.First());
    }
    
    public static async ETTask<List<Entity>> QueryMany(int zone, List<Type> types,List<long> ids) 
    {
        if (types.Count != ids.Count)
        {
            throw new Exception("传递进来的参数不符合预期");
        }

        var dbCacheList = StartSceneService.Instance.ZoneScenes[(zone, SceneType.DBCache)];
        
        List<Entity> result = new List<Entity>();
        Dictionary<int, QueryEntityRequest> index2Query = new Dictionary<int, QueryEntityRequest>();

        for (int i = 0; i < ids.Count; i++)
        {
            var index = ids[i] % dbCacheList.Count;

            if (!index2Query.TryGetValue((int)index, out var request))
            {
                request = new QueryEntityRequest();
                index2Query[(int)index] = request;
                request.Zone = zone;
            }
            
            request.IdList.Add(ids[i]);
            request.TypeList.Add(types[i].FullName);
            request.CollectionNames.Add(types[i].Name);
        }

        foreach (var v in index2Query)
        {
            var sceneActorId = dbCacheList[(int)v.Key].InstanceId;

            // 这里可能最终都访问同一个数据库,就算并发也不会带来效率提升 ,如果不访问,这点效率损失也没啥
            var resp = (QueryEntityResponse) await MessageHelper.CallActor(sceneActorId, v.Value);
            foreach (var vEntity in resp.EntityList)
            {
                result.Add(MongoHelper.Deserialize<Entity>(vEntity));
            }
        }
        
        return result;
    }

    public static async ETTask<List<Entity>> QueryMany(int zone, List<Type> types, long id)
    {

        List<Entity> result = new List<Entity>();

        var dbCacheList = StartSceneService.Instance.ZoneScenes[(zone, SceneType.DBCache)];
        var index = id % dbCacheList.Count;
        QueryEntityRequest request = new QueryEntityRequest();
        request.Zone = zone;

        for (int i = 0; i < types.Count; i++)
        {
            request.IdList.Add(id);
            request.TypeList.Add(types[i].FullName);
            request.CollectionNames.Add(types[i].Name);
        }
        
        var sceneActorId = dbCacheList[(int)index].InstanceId;
        
        var resp = (QueryEntityResponse)await MessageHelper.CallActor(sceneActorId, request);
        foreach (var vEntity in resp.EntityList)
        {
            result.Add(MongoHelper.Deserialize<Entity>(vEntity));
        }
        
        return result;
    }

    public static async ETTask<List<T>> QueryMany<T>(int zone, List<long> ids,string collectionName = null) where T: Entity
    {
        var type = typeof (T);
        if (string.IsNullOrEmpty(collectionName))
            collectionName = type.Name;
        
        var dbCacheList = StartSceneService.Instance.ZoneScenes[(zone, SceneType.DBCache)];
        
        List<T> result = new List<T>();
        Dictionary<int, QueryEntityRequest> index2Query = new Dictionary<int, QueryEntityRequest>();

        for (int i = 0; i < ids.Count; i++)
        {
            var index = ids[i] % dbCacheList.Count;

            if (!index2Query.TryGetValue((int)index, out var request))
            {
                request = new QueryEntityRequest();
                index2Query[(int)index] = request;
                request.Zone = zone;
            }
            
            request.IdList.Add(ids[i]);
            request.TypeList.Add(type.FullName);
            request.CollectionNames.Add(collectionName);
        }

        foreach (var v in index2Query)
        {
            var sceneActorId = dbCacheList[(int)v.Key].InstanceId;

            // 这里可能最终都访问同一个数据库,就算并发也不会带来效率提升 ,如果不访问,这点效率损失也没啥
            var resp = (QueryEntityResponse) await MessageHelper.CallActor(sceneActorId, v.Value);
            foreach (var vEntity in resp.EntityList)
            {
                result.Add(MongoHelper.Deserialize<T>(vEntity));
            }
        }
        
        return result;
    }

    #region 表达式查询
    
    public static async ETTask<T> QueryOne<T>(int zone, Expression<Func<T, bool>> filter, string collection = null)
            where T : Entity
    {
        long result = 0;
        using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandomNumber(0,DBComponent.TaskCount)))
        {
            var cursor = await DBManagerComponent.Instance.GetZoneDB(zone).GetCollection<T>(collection).DistinctAsync(v=>v.Id, filter);
            result = await cursor.FirstOrDefaultAsync();
        }
        if (result == 0)
        {
            return null;
        }

        return await Query<T>(zone,result, collection);
    }

    public static async ETTask<List<T>> QueryMany<T>(int zone,Expression<Func<T, bool>> filter, string collection = null)
            where T : Entity
    {
        List<long> results;
        using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandomNumber(0, DBComponent.TaskCount)))
        {
            var cursor = await DBManagerComponent.Instance.GetZoneDB(zone).GetCollection<T>(collection).DistinctAsync(v=>v.Id,filter);

            results = await cursor.ToListAsync();
        }

        if (results == null || results.Count == 0)
        {
            return new List<T>();
        }

        return await QueryMany<T>(zone, results,collection);
    }
    
    public static async ETTask<List<T>> QueryManyWithCountLimit<T>(int zone,Expression<Func<T, bool>> filter,int countLimit, string collection = null)
            where T : Entity
    {
        List<long> results;
        
        
        
        using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandomNumber(0, DBComponent.TaskCount)))
        {
            var cursor = await DBManagerComponent.Instance.GetZoneDB(zone).GetCollection<T>(collection).DistinctAsync(v=>v.Id,filter);

            results = await cursor.ToListAsync();

            if (results.Count > countLimit)
            {
                results.RemoveRange(countLimit, results.Count - countLimit);
            }
        }

        if (results == null || results.Count == 0)
        {
            return new List<T>();
        }

        return await QueryMany<T>(zone, results,collection);
    }
    
    public static async ETTask<List<T>> QueryJson<T>(int zone, string json, string collection = null) where T : Entity
    {
        List<long> results = null;
        using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandomNumber(0,DBComponent.TaskCount)))
        {
            FilterDefinition<T> filterDefinition = new JsonFilterDefinition<T>(json);
            var cursor = await DBManagerComponent.Instance.GetZoneDB(zone).GetCollection<T>(collection).DistinctAsync(v=>v.Id,filterDefinition);
            results =  await cursor.ToListAsync();
        }
        if (results == null || results.Count == 0)
        {
            return new List<T>();
        }
        return await QueryMany<T>(zone, results,collection);
    }

    #endregion
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="entity"></param>
    /// <param name="saveImd">是否立即存</param>
    /// <typeparam name="T"></typeparam>
    public static async ETTask<int> Save<T>(int zone, T entity,bool saveImd = false) where T:Entity
    {
        return await Save<T>(zone, entity, typeof (T).Name,saveImd);
    }
    
    public static async ETTask SaveNoGeneric(int zone, Entity entity,string collectionName = "") 
    {
        if (entity == null)
        {
            Logger.Error("存储时传入了空Object {Type}",entity.GetType().FullName);
            return;
        }
        
        var dbCacheList = StartSceneService.Instance.ZoneScenes[(zone, SceneType.DBCache)];
        
        entity.Version++;
        var startTime = TimeHelper.ServerNow();
        //Logger.Information("EntityCache写入请求: {Type} {Id} {SceneId} {Version}",entity.GetType().Name,entity.Id,entity.DomainScene()?.Id,entity.Version);
        var request = new EntitySaveRequest { Zone = zone, };
        request.SendTime = startTime;
        request.Entitys.Add(MongoHelper.Serialize(entity));
        if (string.IsNullOrEmpty(collectionName))
            collectionName = entity.GetType().Name;
        request.CollectionNames.Add(collectionName);

        string stackTrace = "";
        
        #if DEBUG
        stackTrace = new StackTrace().ToString();
        #endif
        
        var index = entity.Id % dbCacheList.Count;
        var sceneActorId = dbCacheList[(int)index].InstanceId;
        

        var resp = await MessageHelper.CallActor(sceneActorId, request);
        if (resp.Error != 0)
        {
            Logger.Information("Error: 存库失败 {Type} {Id} {Version} {Error} \n {StackTrace}",entity.GetType().Name,entity.Id,entity.Version, resp.Error,stackTrace);
            entity.Version--;
        }

        var delta = TimeHelper.ServerNow() - startTime;
        if (delta >= 10000)
        {
            Logger.Warning("请求EntityCache服的延迟超过预期 服务器压力也许到达极限. {Type} {Id} {Version} {Delta}", entity.GetType().Name, entity.Id, entity.Version,
                delta);
        }

        // else
        // {
        //     Logger.Information("存库成功 {Type} {Id} \n{StackTrace}",entity.GetType().Name,entity.Id,writeST);
        // }
    }
    
    public static async ETTask<int> Save<T>(int zone, T entity,string collectionName,bool saveImd = false) where T:Entity
    {
        if (entity == null)
        {
            Logger.Error("存储时传入了空Object {Type}",entity.GetType().FullName);
            return -1;
        }
        
        var dbCacheList = StartSceneService.Instance.ZoneScenes[(zone, SceneType.DBCache)];

        if (string.IsNullOrEmpty(collectionName))
            collectionName = typeof(T).Name;

        string stackTrace = "";
        
#if DEBUG
        stackTrace = new StackTrace().ToString();
#endif
        
        entity.Version++;
        var startTime = TimeHelper.ServerNow();
        //Logger.Information("EntityCache写入请求: {Type} {Id} {SceneId} {Version}",entity.GetType().Name,entity.Id,entity.DomainScene()?.Id,entity.Version);
        
        var request = new EntitySaveRequest { Zone = zone, };
        request.SendTime = startTime;
        request.Entitys.Add(MongoHelper.Serialize(entity));
        request.CollectionNames.Add(collectionName);
        request.SaveImd = saveImd;

        var index = entity.Id % dbCacheList.Count;
        var sceneActorId = dbCacheList[(int)index].InstanceId;

        var resp = await MessageHelper.CallActor(sceneActorId, request);
        
        var delta = TimeHelper.ServerNow() - startTime;
        if (delta >= 10000)
        {
            Logger.Warning("请求EntityCache服的延迟超过预期 服务器压力也许到达极限. {Type} {Id} {Version} {Delta}", entity.GetType().Name, entity.Id,
                entity.Version, delta);
        }
        
        if (resp.Error != 0)
        {
            entity.Version--;
            Logger.Information("Error: 存库失败 {Type} {Id} {Version} {Error} \n {StackTrace}",entity.GetType().Name,entity.Id, entity.Version+1, resp.Error,stackTrace);
            return resp.Error;
        }


        return 0;
        // else
        // {
        //     Logger.Information("存库成功 {Type} {Id}  \n{StackTrace}",entity.GetType().Name,entity.Id,writeST);
        // }
    }

    public static async ETTask SaveMany(int zone, List<Entity> datas)
    {
        List<ETTask> etTasks = new();
        foreach (var v in datas)
        {
            etTasks.Add(SaveNoGeneric(zone,v));   
        }

        await ETTaskHelper.WaitAll(etTasks);
    }

    public static async ETTask RemoveCache<T>(int zone, long id,string collectionName="") where T : Entity
    {
        var dbCacheList = StartSceneService.Instance.ZoneScenes[(zone, SceneType.DBCache)];
        
        var request = new DeleteEntityRequest { Zone = zone, };
        if (string.IsNullOrEmpty(collectionName))
            collectionName = typeof (T).Name;
        request.Collection = collectionName;
        request.Type = typeof (T).FullName;
        request.DeleteFromDB = false;
        request.Id = id;

        var index = id % dbCacheList.Count;
        var sceneActorId = dbCacheList[(int)index].InstanceId;

        await MessageHelper.CallActor(sceneActorId, request);
    }
    
    public static async ETTask Delete<T>(int zone, long id,string collectionName = "") where T : Entity
    {
        var dbCacheList = StartSceneService.Instance.ZoneScenes[(zone, SceneType.DBCache)];
        
        var request = new DeleteEntityRequest { Zone = zone, };
        if (string.IsNullOrEmpty(collectionName))
            collectionName = typeof (T).Name;
        request.Collection = collectionName;
        request.Type = typeof (T).FullName;
        request.DeleteFromDB = true;
        request.Id = id;

        var index = id % dbCacheList.Count;
        var sceneActorId = dbCacheList[(int)index].InstanceId;

        await MessageHelper.CallActor(sceneActorId, request);
    }
}
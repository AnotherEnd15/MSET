using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using MongoDB.Driver;

namespace ET.Server
{
	[FriendOf(typeof(DBComponent))]
    public static class DBComponentSystem
    {
	    public class DBComponentAwakeSystem : AwakeSystem<DBComponent, string, string>
	    {
			protected override void Awake(DBComponent self, string dbConnection, string dbName)
		    {
			    self.mongoClient = new MongoClient(dbConnection);

#if DEBUG
			    if (File.Exists("../db_prefix.txt"))
			    {
				    var prefix = File.ReadAllText("../db_prefix.txt").Trim();
				    dbName = prefix + "_" + dbName;
			    }
#endif

			    self.DBName = dbName;
			    
			    Log.GetLogger().Information("DB: {Zone} {Name}",self.Id,dbName);

			    self.database = self.mongoClient.GetDatabase(dbName);

			    var nameList = self.database.ListCollectionNames().ToList();
			    foreach (var v in nameList)
			    {
				    self.CollectionNames.Add(v);
			    }
		    }
	    }

	    public static void RefreshCollectionNames(this DBComponent self)
	    {
		    var nameList = self.database.ListCollectionNames().ToList();
		    foreach (var v in nameList)
		    {
			    self.CollectionNames.Add(v);
		    }
	    }

	    public static IMongoCollection<T> GetCollection<T>(this DBComponent self, string collection = null)
	    {
		    var name = collection ?? typeof (T).Name;
		    if (!self.CollectionNames.Contains(name))
		    {
			    if (Options.Instance.Develop > 0)
				    Log.GetLogger().Error($"对应表格没有初始化: {self.Id} {name} \n{new StackTrace().ToString()}");
			    else throw new Exception($"对应表格没有初始化: {self.Id} {name}");

		    }
		    return self.database.GetCollection<T>(name);
	    }

	    public static IMongoCollection<Entity> GetCollection(this DBComponent self, string name)
	    {
		    if (!self.CollectionNames.Contains(name))
		    {
			    if (Options.Instance.Develop > 0)
				    Log.GetLogger().Error($"对应表格没有初始化: {self.Id} {name} \n{new StackTrace().ToString()}");
			    else throw new Exception($"对应表格没有初始化: {self.Id} {name}");
		    }

		    return self.database.GetCollection<Entity>(name);
	    }
	    
	    #region Query
	    
	    public static async ETTask<T> QueryOneNotEntity<T>(this DBComponent self, Expression<Func<T, bool>> filter, string collection = null)
			    where T : Object
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandInt64() % DBComponent.TaskCount))
		    {
			    IAsyncCursor<T> cursor = await self.GetCollection<T>(collection).FindAsync(filter);

			    var list = await cursor.ToListAsync();
			    if (list == null || list.Count == 0)
				    return null;
			    return list.FirstOrDefault();
		    }
	    }
	    
	    public static async ETTask<List<T>> QueryManyNotEntity<T>(this DBComponent self, Expression<Func<T, bool>> filter, string collection = null)
			    where T : Object
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandInt64() % DBComponent.TaskCount))
		    {
			    IAsyncCursor<T> cursor = await self.GetCollection<T>(collection).FindAsync(filter);
			    var list = await cursor.ToListAsync();
			    return list;
		    }
	    }

	    public static async ETTask<List<long>> QueryIds<T>(this DBComponent self, Expression<Func<T, bool>> filter, string collection = null) where T: Entity
	    {
		    List<long> results;
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandomNumber(0, DBComponent.TaskCount)))
		    {
			    var cursor = await self.GetCollection<T>(collection).DistinctAsync(v=>v.Id,filter);

			    results = await cursor.ToListAsync();
		    }
		    return results;
	    }


        // public static async ETTask<List<T>> QueryJson<T>(this DBComponent self, string json, string collection = null) where T : Entity
        // {
        //  using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandInt64() % DBComponent.TaskCount))
        //  {
        //   FilterDefinition<T> filterDefinition = new JsonFilterDefinition<T>(json);
        //   IAsyncCursor<T> cursor = await self.GetCollection<T>(collection).FindAsync(filterDefinition);
        //   return await cursor.ToListAsync();
        //  }
        // }

        // public static async ETTask<List<T>> QueryJson<T>(this DBComponent self, long taskId, string json, string collection = null) where T : Entity
        // {
        //  using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandInt64() % DBComponent.TaskCount))
        //  {
        //   FilterDefinition<T> filterDefinition = new JsonFilterDefinition<T>(json);
        //   IAsyncCursor<T> cursor = await self.GetCollection<T>(collection).FindAsync(filterDefinition);
        //   return await cursor.ToListAsync();
        //  }
        // }

        #endregion


        #region Save

        public static async ETTask SaveNotEntity<T>(this DBComponent self,Expression<Func<T,bool>> filter, T entity, string collection = null) where T : Object
	    {
		    if (entity == null)
		    {
			    Log.GetLogger().Error($"save entity is null: {typeof (T).Name}");

			    return;
		    }
		    
		    if (collection == null)
		    {
			    collection = entity.GetType().Name;
		    }

		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandomNumber(0,DBComponent.TaskCount)))
		    {
			    await self.GetCollection<T>(collection).ReplaceOneAsync<T>(filter, entity, new ReplaceOptions { IsUpsert = true });
		    }
	    }

	    #endregion

	    #region Remove
	    
	    public static async ETTask<long> RemoveOneNotEntity<T>(this DBComponent self, Expression<Func<T, bool>> filter, string collection = null) where T: Object
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandomNumber(0,DBComponent.TaskCount)))
		    {
			    DeleteResult result = await self.GetCollection<T>(collection).DeleteOneAsync(filter);

			    return result.DeletedCount;
		    }
	    }
	    
	    public static async ETTask<long> RemoveManyNotEntity<T>(this DBComponent self, Expression<Func<T, bool>> filter, string collection = null) where T: Object
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomGenerator.RandomNumber(0,DBComponent.TaskCount)))
		    {
			    DeleteResult result = await self.GetCollection<T>(collection).DeleteManyAsync(filter);

			    return result.DeletedCount;
		    }
	    }
	    
	    #endregion
    }
}
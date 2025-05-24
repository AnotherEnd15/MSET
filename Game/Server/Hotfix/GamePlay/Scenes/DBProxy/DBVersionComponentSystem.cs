using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ET;
using ET.DBProxy;
using MongoDB.Driver;
using Serilog;

namespace ET;



public static class DBVersionComponentSystem
{
    private static ILogger Logger = Log.GetLogger();

    [ObjectSystem]
    public static void Awake(this DBVersionComponent self)
    {
        self.AllVersionHandlers.Clear();
        foreach (var v in EventSystem.Instance.GetTypes(typeof(DBVersionAttribute)))
        {
            var attr = v.GetCustomAttribute<DBVersionAttribute>();
            var instance = Activator.CreateInstance(v) as IDBVersionHandler;
            if (instance == null)
            {
                throw new Exception("标记了DBVersionAttribute的类没有DBVersionHandler");
            }

            self.AllVersionHandlers[attr.DBVersion] = instance;
        }
    }
    
    public static async ETTask Init(this DBVersionComponent self,int zone)
    {
        var db = DBProxyComponent.Instance._database;
        var dbNames = await (await db.ListCollectionNamesAsync()).ToListAsync();
        var dbNameSet = new HashSet<string>();
        foreach (var v in dbNames)
        {
            dbNameSet.Add(v);
        }

        if (!dbNameSet.Contains(nameof (DBVersion)))
        {
            await db.CreateCollectionAsync(nameof (DBVersion));
            dbNameSet.Add(nameof (DBVersion));
        }

        var collection = db.GetCollection<DBVersion>(nameof (DBVersion));

        var versionData = await collection.FindAsync(Builders<DBVersion>.Filter.Where(v => true));
        var dbVersion = (await versionData.ToListAsync()).FirstOrDefault();
        if (dbVersion == null)
        {
            await DBInit.Init(zone, db, dbNameSet);
            await collection.InsertOneAsync(new DBVersion() { Version = DBVersionEnum.DB_Max - 1});
            Logger.Information("数据库初始化完毕 {Zone} 当前版本{DBVersionEnum}",zone, DBVersionEnum.DB_Max - 1);
        }
        else
        {

            int startVersion = (int)(dbVersion.Version + 1);
            #if DEBUG
            startVersion = 1;
            Logger.Information("本地开发模式 数据库版本自动从0再次升级");
            #endif
            
            if (startVersion >= (int)DBVersionEnum.DB_Max - 1)
            {
                Logger.Information("数据库无需升级 跳过 {Zone} {Version}",zone,dbVersion.Version);
                return;
            }
            

            for (int i = startVersion; i <= (int)(DBVersionEnum.DB_Max - 1); i++)
            {
                var key = (DBVersionEnum)i;
                if (!self.AllVersionHandlers.TryGetValue(key, out var handler))
                {
                    continue;
                }
                await handler.HandleAsync(zone, db, dbNameSet);
                dbVersion.Version = (DBVersionEnum)i;
                await collection.UpdateOneAsync(Builders<DBVersion>.Filter.Eq(ver => ver.Id, dbVersion.Id),
                    Builders<DBVersion>.Update.Set(ver => ver.Version, dbVersion.Version));
                Logger.Information("数据库升级完毕 {Zone} 当前版本{DBVersionEnum}",zone, (DBVersionEnum)i);
                
            }
        }

    }
    
}
using System;
using System.Collections.Generic;
using ET.GamePlay;
using ET.GMTool;
using ET.Server;
using MongoDB.Driver;

namespace ET;



public static class DBInit
{
    /// <summary>
    ///  指定区的数据库初始化,只会调用一次. 业务开发需要保证DBInit中永远是最新的库的数据 DBVersion做旧库升级用
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="database"></param>
    public static async ETTask Init(int zone, IMongoDatabase database, HashSet<string> existCollections)
    {
        // todo 分多区情况下要处理不同区分别初始化不同的表和索引
        
        await database.CreateCollectionIfAbsent<ServerSetting>(existCollections);

        await database.CreateCollectionIfAbsent<BanAccount>(existCollections);

        await database.CreateCollectionIfAbsent<ServerManageComponent>(existCollections);

        await database.CreateCollectionIfAbsent<WhitelistAccount>(existCollections);

        await database.CreateCollectionIfAbsent<Player>(existCollections);
        {
            var indeKey1 = Builders<Player>.IndexKeys.Hashed(v => v.Id);
            await database.CreateIndex(nameof(Player), indeKey1);
        }
    }
}
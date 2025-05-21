using System.Collections.Generic;
using MongoDB.Driver;

namespace ET;

/// <summary>
/// 旧库数据升级用
/// </summary>
public interface IDBVersionHandler
{
    ETTask HandleAsync(int zone,IMongoDatabase database,HashSet<string> existCollections);
}
using System.Collections.Generic;
using MongoDB.Driver;

namespace ET.Server
{
    /// <summary>
    /// 用来缓存数据
    /// </summary>
    [ChildOf(typeof(DBManagerComponent))]
    public class DBComponent: Entity, IAwake<string, string>, IDestroy
    {
        public const int TaskCount = 96;

        public MongoClient mongoClient;
        public IMongoDatabase database;
        public string DBName;

        public HashSet<string> CollectionNames = new();
    }
}
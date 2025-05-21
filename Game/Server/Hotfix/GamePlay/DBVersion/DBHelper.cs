using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace ET
{
    public static class DBHelper
    {
        public static async ETTask CreateCollectionIfAbsent<T>(this IMongoDatabase database, HashSet<string> existCollections)
        {
            var name = typeof (T).Name;
            await CreateCollectionIfAbsent(database, name, existCollections);
        }

        public static async ETTask CreateCollectionIfAbsent(this IMongoDatabase database, string name, HashSet<string> existCollections)
        {
            if (existCollections.Contains(name))
                return;
            existCollections.Add(name);
            await database.CreateCollectionAsync(name);
        }

        public static async ETTask CreateIndex<T>(this IMongoDatabase database, IndexKeysDefinition<T> indexKeysDefinition)
        {
            var collection = database.GetCollection<T>(typeof (T).Name);
            var indexModel = new CreateIndexModel<T>(indexKeysDefinition);
            await collection.Indexes.CreateOneAsync(indexModel);
        }

        public static async ETTask CreateIndex<T>(this IMongoDatabase database, string collectionName, IndexKeysDefinition<T> indexKeysDefinition)
        {
            var collection = database.GetCollection<T>(collectionName);
            var indexModel = new CreateIndexModel<T>(indexKeysDefinition);
            await collection.Indexes.CreateOneAsync(indexModel);
        }

        public static async ETTask CreateIndex<T>(this IMongoDatabase database, string collectionName, CreateIndexModel<T> model)
        {
            var collection = database.GetCollection<T>(collectionName);
            await collection.Indexes.CreateOneAsync(model);
        }

        public static async ETTask CreateExpireIndex<T>(this IMongoDatabase database, string collectionName, IndexKeysDefinition<T> expireIndex)
        {
            var indexModel = new CreateIndexModel<T>(keys: expireIndex,
                options: new CreateIndexOptions { ExpireAfter = TimeSpan.FromSeconds(0), Name = "ExpireAtIndex" });
            var collection = database.GetCollection<T>(collectionName);
            await collection.Indexes.CreateOneAsync(indexModel);
        }
    }
}
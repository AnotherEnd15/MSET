using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.BehaviorTree
{
    public class BehaviorTree
    {
        [BsonId]
        public string Id;
        public RootNode RootNode;

        public BehaviorTree(string id)
        {
            this.Id = id;
        }
    }
}
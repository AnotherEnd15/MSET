using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.GamePlay
{
    
    public class ServerSetting : Object
    {
        [BsonId]
        public int Zone;
        
        public int ServerStateHighUsageLine; // 繁忙线(不强制的时候有用)
        public bool ForceHighUsage; // 是否强制显示繁忙
    }
}
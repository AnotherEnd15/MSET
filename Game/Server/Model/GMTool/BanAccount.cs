using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.GMTool
{
    public class BanAccount : Object
    {
        [BsonId]
        public string OpenId;
        
        public long ExpireTime;
    }
}
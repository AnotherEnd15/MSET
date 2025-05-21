using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.GMTool
{
    public class WhitelistAccount : Object
    {
        [BsonId]
        public long Id;

        public List<string> OpenIdList = new();
    }
}
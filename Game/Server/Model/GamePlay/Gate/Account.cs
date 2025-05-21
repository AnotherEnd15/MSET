using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public class Account : Object
    {
        [BsonId]
        public string UserName;
        public int Uid; // 递增的玩家id 一般从100000开始
    }

    public class LoginLock
    {
        [BsonId]
        public int Uid;
        public long SceneId;
    }
}

using ET.Define;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ChildOf(typeof(PlayerComponent))]
    public sealed class Player : Entity, IAwake
    {
        public int Zone { get; set; }

        [BsonIgnore]
        public long GateActorId; // 发给Gate上的GatePlayer
        [BsonIgnore]
        public long ClientSessionId; // 发客户端消息专用
    }
}
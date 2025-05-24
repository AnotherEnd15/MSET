using ET.Define;
using MongoDB.Bson.Serialization.Attributes;
using SyncFramework;
using System.Collections.Generic;

namespace ET
{
    [ChildOf(typeof(PlayerComponent))]
    [SyncFramework.SyncClass]
    public sealed partial class Player : Entity, IAwake
    {
        public int Zone { get; set; }

        [SyncProperty]
        public partial string Name { get; set; }

        // 使用普通集合（会在整个替换时发送Clear操作）
        [SyncProperty]
        public partial List<string> NameList { get; set; }

        [SyncProperty]
        public partial Dictionary<string, string> NameDict { get; set; }

        // 推荐：使用同步集合类型（自动追踪每个操作）
        [SyncProperty]
        public partial SyncList<string> SyncNameList { get; set; }

        [SyncProperty]
        public partial SyncDictionary<string, int> SyncStats { get; set; }

        [BsonIgnore]
        public long GateActorId; // 发给Gate上的GatePlayer
        [BsonIgnore]
        public long ClientSessionId; // 发客户端消息专用
    }
}
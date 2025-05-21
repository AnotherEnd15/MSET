using System.Collections.Generic;
using MongoDB.Bson;

namespace ET;

// 每个类型的Entity都对应一个Cache缓存
[ChildOf(typeof(EntityCacheComponent))]
public class EntityCache : Entity,IAwake,IDestroy
{
    public int Zone { get; set; } // 对应哪个区的
    public string CollectionName { get; set; } // 对应表名

    // 每次添加/更新/查询时, 对应的链表节点移动到链表头部 需要删除的时候,从链表尾部开始删除即可
    public Dictionary<long, LinkedListNode<Entity>> CacheEntity = new ();
    public LinkedList<Entity> RecentlyUsed = new();
    
    public Dictionary<long, long> EntityOpTime = new(); // 实体操作的数量

    public HashSet<long> InSavingEntity = new(); // 还没保存过的Entity

    //public long RemoveTimer;
}
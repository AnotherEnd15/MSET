using System;
using System.Collections.Generic;

namespace ET;

[ComponentOf(typeof(Scene))]
public class EntityCacheComponent : Entity,IAwake,IDestroy,IUpdate
{
    public Dictionary<(int zone,Type type,string collectionName), EntityCache> AllCaches = new();

    public Queue<(EntityCache entityCache, long entityId, long version)> SaveQueue = new();

    // 组件id和DB保存队列的映射 保证一个相对平均的分布
    public Dictionary<long, int> Id2DBQueueMap = new();

    public int LastCacheId = -1;

    public long DBSavingCount; // 正在保存中的数量

    public long LastLogTime;
    public int SaveOpCount;
}
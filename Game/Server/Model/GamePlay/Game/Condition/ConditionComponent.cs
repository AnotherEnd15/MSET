using System;
using System.Collections.Generic;
using ET.Server;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    [ComponentOf(typeof(Player))]
    public class ConditionComponent : Entity,IAwake
    {
        public Dictionary<ConditionType, HashSet<CondListenerGroup>> CondType2ListenerGroups = new();
    }

    // 每个模块可能需要自己存库的数据 记录每个条件在统计系统里的初始值
    public class ConditionGroupStartValueRecord : Object
    {
        // key是conditionId value是起始进度
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> Progress = new();
    }


    // 代表着一组条件的监听和处理
    [ChildOf(typeof (ConditionComponent))]
    public class CondListenerGroup : Entity, IAwake
    {
        public int GroupConfigId => (int)this.Id;
        public ConditionGroupConfig GroupConfig => ConditionGroupConfigCategory.Instance.Get(this.GroupConfigId);
        public List<OneCondListener> ConditionListeners = new(); // 每个条件组可能依赖多种统计条件
    }

    public class OneCondListener
    {
        public int ConditionId;
        public ConditionConfig ConditionConfig => ConditionConfigCategory.Instance.Get(this.ConditionId);
        
        public long StartValue;
        public long NeedValue;
    }
}
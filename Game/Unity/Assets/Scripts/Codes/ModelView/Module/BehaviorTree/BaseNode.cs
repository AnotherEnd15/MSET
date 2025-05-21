using System;
using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ET.BehaviorTree
{
    [Serializable]
    public abstract partial class BaseNode
    {
        [BsonId]
        [ReadOnly]
        public string Id;
        [LabelText("注释")]
        public string Comment = "";
        
        [ReadOnly]
        public NodeType NodeType = NodeType.Action;


        [BsonIgnore]
        [NonSerialized]
        public CompNode Parent;

        protected BaseNode()
        {
            this.Id = Guid.NewGuid().ToString("N");
        }

        public virtual void OnDeserialize()
        {
            
        }
    }

    public abstract partial class CompNode: BaseNode
    {
        protected CompNode(): base()
        {
            this.NodeType = NodeType.Comp;
        }
        
        [SerializeReference]
        public List<BaseNode> Childs = new();

        public override void OnDeserialize()
        {
            foreach (var v in this.Childs)
            {
                v.Parent = this;
            }
        }
    }


    public enum NodeType
    {
        Action,
        Comp,
        Root,
    }

    public enum NodeCategory
    {
        Normal,
        View // 显示层 Unity用
    }

}
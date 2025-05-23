using System;
using System.ComponentModel;

namespace ET
{
    public abstract class ProtoObject: Object, ISupportInitialize
    {
        public virtual void BeginInit()
        {
        }
        
        
        public virtual void EndInit()
        {
        }
        
        
        public virtual void AfterEndInit()
        {
        }
    }
}
using System;
using ET.Server;

namespace ET
{
    public interface IConditionHandler
    {
        void Handle(Entity parent, ConditionConfig condConfig);
        
    }

    
    public abstract class AConditionHandler : IConditionHandler 
    {
        public void Handle(Entity parent, ConditionConfig condConfig)
        {
            Run(parent as Player, condConfig);
        }

        protected abstract void Run(Player unit, ConditionConfig config);
    }
}
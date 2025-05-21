using System;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ConditionMgrComponent : Entity,IAwake,ILoad
    {
        [StaticField]
        public static ConditionMgrComponent Instance;

        public Dictionary<ConditionType, IConditionHandler> AllConditionHandler = new();
    }
}
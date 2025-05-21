using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class StateMachineComponent : Entity,IAwake,ILoad
    {
        [StaticField]
        public static StateMachineComponent Instance;
        
        public Dictionary<int, IStateMachine> StateMachines = new();
    }
}
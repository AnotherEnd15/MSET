using System;

namespace ET
{
    public static class StateMachineComponentSystem
    {
        [ObjectSystem]
        public static void Awake(this StateMachineComponent self)
        {
            StateMachineComponent.Instance = self;
            self.LoadInternal();
        }
        
        [ObjectSystem]
        public static void Load(this StateMachineComponent self)
        {
            self.LoadInternal();
        }
        
        private static void LoadInternal(this StateMachineComponent self)
        {
            self.StateMachines.Clear();
            foreach (var v in EventSystem.Instance.GetTypes(typeof(StateMachineAttribute)))
            {
                if(v.IsAbstract || !v.IsClass)
                    continue;

                var attr = v.GetCustomAttributes(typeof (StateMachineAttribute), false)[0] as StateMachineAttribute;
                var instance = Activator.CreateInstance(v) as IStateMachine;
                self.StateMachines.Add(attr.StateType,instance);
            }
        }


        public static IStateMachine GetStateMachine(this StateMachineComponent self, int type)
        {
            return self.StateMachines[type];
        }
    }
}
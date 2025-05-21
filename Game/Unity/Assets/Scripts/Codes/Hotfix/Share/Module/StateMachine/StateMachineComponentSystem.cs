using System;

namespace ET
{
    public static class StateMachineComponentSystem
    {
        public class AwakeSystem : AwakeSystem<StateMachineComponent>
        {
            protected override void Awake(StateMachineComponent self)
            {
                StateMachineComponent.Instance = self;
                self.Load();
            }
        }
        
        public class LoadSystem : LoadSystem<StateMachineComponent>
        {
            protected override void Load(StateMachineComponent self)
            {
                self.Load();
            }
        }
        
        public static void Load(this StateMachineComponent self)
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
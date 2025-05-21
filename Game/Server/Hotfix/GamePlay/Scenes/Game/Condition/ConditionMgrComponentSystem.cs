using System;

namespace ET.Module.Condition
{
    public static class ConditionMgrComponentSystem
    {
        public class AwakeSystem: AwakeSystem<ConditionMgrComponent>
        {
            protected override void Awake(ConditionMgrComponent self)
            {
                ConditionMgrComponent.Instance = self;
                self.Load();
            }
        }

        public class LoadSystem: LoadSystem<ConditionMgrComponent>
        {
            protected override void Load(ConditionMgrComponent self)
            {
                self.Load();
            }
        }

        public static void Load(this ConditionMgrComponent self)
        {
            self.AllConditionHandler.Clear();
            foreach (var v in EventSystem.Instance.GetTypes(typeof(ConditionAttribute)))
            {
                if (v.IsAbstract || !v.IsClass)
                {
                    continue;
                }

                var attr = v.GetCustomAttributes(typeof (ConditionAttribute), false)[0] as ConditionAttribute;
                
                var handler = Activator.CreateInstance(v) as IConditionHandler;
                
                self.AllConditionHandler.Add(attr.ConditionType,handler);
            }
        }
    }
}
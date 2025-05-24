using System;

namespace ET.Module.Condition
{
    public static class ConditionMgrComponentSystem
    {
        [ObjectSystem]
        public static void Awake(this ConditionMgrComponent self)
        {
            ConditionMgrComponent.Instance = self;
            self.LoadInternal();
        }

        [ObjectSystem]
        public static void Load(this ConditionMgrComponent self)
        {
            self.LoadInternal();
        }

        private static void LoadInternal(this ConditionMgrComponent self)
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
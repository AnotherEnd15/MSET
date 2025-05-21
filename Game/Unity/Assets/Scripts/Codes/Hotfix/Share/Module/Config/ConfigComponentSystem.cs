using System;
using System.Collections.Generic;
using Luban;

namespace ET
{
    public static class ConfigComponentSystem
    {
        
        public static void RunConfigCheck(this ConfigComponent self)
        {
            self.ConfigChecks.Clear();
            foreach (var v in EventSystem.Instance.GetTypes(typeof(ConfigCheckAttribute)))
            {
                if(v.IsAbstract || !v.IsClass)
                    continue;
                self.ConfigChecks.Add(Activator.CreateInstance(v) as IConfigCheck);
            }

            foreach (var v in self.ConfigChecks)
            {
                v.Run().Coroutine();
            }
        }
        
        public static void LoadAll(this ConfigComponent self)
        {
            var ret = EventSystem.Instance.Invoke<ET.ConfigComponent.GetAllConfigBytes, ET.ConfigResult>(new ConfigComponent.GetAllConfigBytes());
            self.LoadAll(ret);
        }
        
        public static void LoadAll(this ConfigComponent self, ConfigResult ret)
        {
            var table = new Tables(v => new ByteBuf(ret.Configs[v]));
        }

        public static void LoadOne(this ConfigComponent self, Type type, ByteBuf byteBuf)
        {
            var instance = Activator.CreateInstance(type) as CategoryBase;
            instance.Load(byteBuf);
        }
        
    }
}
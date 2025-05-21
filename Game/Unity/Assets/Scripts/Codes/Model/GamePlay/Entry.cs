using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using MemoryPack.Internal;

namespace ET
{
    namespace EventType
    {
        public struct LogicEntryEvent
        {
            
        }
        
    }
    
    [Preserve]
    // 客户端和服务器共用
    public static class Entry
    {
        public static void Init()
        {
            Game.AddSingleton<EventSystem>();
            Game.AddSingleton<TimerComponent>();
            Game.AddSingleton<CoroutineLockComponent>();
        }
        
        public static void Start()
        {
            StartAsync().Coroutine();
        }

        private static async ETTask StartAsync()
        {
            Game.AddSingleton<Root>();
            Game.AddSingleton<NetServices>();
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.LogicEntryEvent());
        }


        // 客户端专用 一次性全流程
        public static void Run(List<Assembly> assemblies)
        {
            Init();
            
            assemblies.Add(typeof (Game).Assembly);
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies.ToArray());
            
            EventSystem.Instance.Add(types);

            Start();
        }

        public static void AddTypes(Dictionary<string, Type> types)
        {
            EventSystem.Instance.Add(types);
        }
    }
}
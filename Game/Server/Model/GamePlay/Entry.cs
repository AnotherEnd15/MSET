using System;
using System.Collections.Generic;
using System.Reflection;

namespace ET
{
    namespace EventType
    {
        public struct EntryEvent
        {
        }
    }
    
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
            WinPeriod.Init();
            
            Game.AddSingleton<NetServices>();
            Game.AddSingleton<Root>();
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent());
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
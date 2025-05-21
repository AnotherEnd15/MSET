using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using ET.GamePlay;
using ET.Helper;
using ET.Module;
using ET.Module.Quartz;
using MongoDB.Driver;

namespace ET.Server
{
    [Event]
    public class EntryEvent_InitServer: AEvent<Scene,ET.EventType.EntryEvent>
    {
        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent args)
        {
            MongoHelper.Init();

            Root.Instance.Scene.AddComponent<NetThreadComponent>();
            Root.Instance.Scene.AddComponent<OpcodeTypeComponent>();
            Root.Instance.Scene.AddComponent<MessageDispatcherComponent>();
            Root.Instance.Scene.AddComponent<ClientSceneManagerComponent>();
            Root.Instance.Scene.AddComponent<DBManagerComponent>();
            
            
            Game.AddSingleton<ConfigComponent>().LoadAll();
            StartHelper.LoadStartConfig(Options.Instance.Process);
            await Game.AddSingleton<EtcdManager>().InitAsync();
            Game.AddSingleton<RedisManager>();
            ConfigComponent.Instance.RunConfigCheck();
            
            // 发送普通actor消息
            Root.Instance.Scene.AddComponent<ActorMessageSenderComponent>();
            
            Root.Instance.Scene.AddComponent<ActorMessageDispatcherComponent>();
            Root.Instance.Scene.AddComponent<ServerSceneManagerComponent>();
            Root.Instance.Scene.AddComponent<RobotCaseComponent>();
            Root.Instance.Scene.AddComponent<ConditionMgrComponent>();
            Root.Instance.Scene.AddComponent<StateMachineComponent>();
            Root.Instance.Scene.AddComponent<SceneControllerComponent>();

            await Root.Instance.Scene.AddComponent<QuartzComponent>().Init();

            var scenes = await StartHelper.CreateScenes(Options.Instance.Process);
            
            // TODO 每个Scene创建后都有自己的依赖, 汇聚在一起 就是进程的依赖
            // 暂时先等待全部进程注册完毕
            while (true)
            {
                if (ServerConfig.Instance.StartConfig.SceneConfigs.Count != StartSceneService.Instance.AllConfigs.Count)
                {
                    Log.GetLogger().Information("等待依赖进程注册到etcd.....");
                    await TimerComponent.Instance.WaitAsync(2000);
                }
                break;
            }

            foreach (var v in scenes)
            {
                await SceneControllerComponent.Instance.InitScene(v);
            }
            
            if (Options.Instance.Console == 1)
            {
                Root.Instance.Scene.AddComponent<ConsoleComponent>();
            }

            if (Options.Instance.Develop > 0)
            {
                Root.Instance.Scene.AddComponent<GMComponent>();
            }
            
            Log.GetLogger().Information("服务器初始化完毕");
        }
    }
}
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
using YamlDotNet.RepresentationModel;

namespace ET.Server
{
    [Event]
    public class EntryEvent_InitServer : AEvent<Scene, ET.EventType.EntryEvent>
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
            // 每个Scene创建后都有自己的依赖, 汇聚在一起 取最大值 就是进程的依赖
            var dependency = CalculateTotalServiceDependencies(ServerConfig.Instance.Config["services_dependency"], 
                scenes.Select(v => v.SceneType));
            while (true)
            {
                int enoughCount = 0;
                foreach (var need in dependency)
                {
                    if (!StartSceneService.Instance.ZoneScenes.TryGetValue((1, need.Key), out var targetScenes))
                    {
                        continue;
                    }
                    if (targetScenes.Count >= need.Value)
                    {
                        enoughCount++;
                    }
                }

                if (enoughCount < dependency.Count)
                {
                    Log.GetLogger().Information("等待依赖进程注册到etcd.....");
                    await TimerComponent.Instance.WaitAsync(2000);
                    continue;
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


        /// <summary>
        /// 解析服务依赖并计算总需求量
        /// </summary>
        /// <param name="dependencyNode">服务依赖的YamlNode对象</param>
        /// <param name="servicesList">已有的服务列表</param>
        /// <returns>字典，键为服务名称，值为需要的最小数量</returns>
        public Dictionary<SceneType, int> CalculateTotalServiceDependencies(YamlNode dependencyNode, IEnumerable<SceneType> servicesList)
        {
            // 结果字典：服务名称 -> 最小需要数量
            Dictionary<SceneType, int> totalDependencies = new Dictionary<SceneType, int>();

            // 检查节点是否有效
            if (dependencyNode == null || !(dependencyNode is YamlMappingNode))
            {
                Console.WriteLine("依赖配置无效或为空");
                return totalDependencies;
            }

            YamlMappingNode dependencyMapping = (YamlMappingNode)dependencyNode;

            // 1. 首先获取基础依赖（适用于所有服务）
            YamlNode baseNode = null;
            if (dependencyMapping.Children.TryGetValue(new YamlScalarNode("__Base"), out baseNode) && baseNode is YamlMappingNode)
            {
                YamlMappingNode baseDeps = (YamlMappingNode)baseNode;
                foreach (var dep in baseDeps.Children)
                {
                    string serviceName = dep.Key.ToString();
                    int count = int.Parse(dep.Value.ToString());

                    // 将基础依赖添加到结果中
                    totalDependencies[Enum.Parse<SceneType>(serviceName)] = count;
                }
            }

            // 2. 处理每个服务的特定依赖
            foreach (var service in servicesList)
            {
                YamlNode serviceNode = null;
                if (dependencyMapping.Children.TryGetValue(new YamlScalarNode(service.ToString()), out serviceNode) && serviceNode is YamlMappingNode)
                {
                    YamlMappingNode serviceDeps = (YamlMappingNode)serviceNode;
                    foreach (var dep in serviceDeps.Children)
                    {
                        var dependencyName = Enum.Parse<SceneType>(dep.Key.ToString());
                        int count = int.Parse(dep.Value.ToString());

                        // 如果已存在这个依赖，取最大值
                        if (totalDependencies.ContainsKey(dependencyName))
                        {
                            totalDependencies[dependencyName] = Math.Max(totalDependencies[dependencyName], count);
                        }
                        else
                        {
                            totalDependencies[dependencyName] = count;
                        }
                    }
                }
            }

            return totalDependencies;
        }
    }
}
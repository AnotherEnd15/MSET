using System;
using System.Collections.Generic;
using System.IO;
using ET;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ET.Helper;

public static class StartHelper
{
    public static void LoadStartConfig(int process)
    {
        var configFolder = $"../Config/{Options.Instance.StartConfig}";
        // 根据configs生成一个yaml的配置文件
        // 创建序列化器
        var serializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var serverConfig = Path.Combine(configFolder, "server_config.yaml");
        var startSceneConfig = Path.Combine(configFolder, "start_scenes.yaml");

        StartConfig startConfig =
            serializer.Deserialize<StartConfig>(File.ReadAllText(startSceneConfig));
        ServerConfig.Instance = new ServerConfig(File.ReadAllText(serverConfig), startConfig);
        Log.GetLogger().Information($"host_namespace: {ServerConfig.Instance.HostName}");
    }

    public static StartProcessConfig GetProcessConfig(this StartSceneConfig startSceneConfig)
    {
        return ServerConfig.Instance.StartConfig.ProcessConfigs[startSceneConfig.Process];
    }

    public static async ETTask<List<Scene>> CreateScenes(int process)
    {
        List<Scene> scenes = new List<Scene>();
        foreach (var v in ServerConfig.Instance.StartConfig.SceneConfigs)
        {
            if (v.Process != process)
            {
                continue;
            }
            var id = IdGenerater.Instance.GenerateId(Options.Instance.Process);
            var instanceId = IdGenerater.Instance.GenerateInstanceId(Options.Instance.Process);
            var name = $"{v.SceneType}_{instanceId}"; // TODO: start scene中新增name的配置
            var sceneType = Enum.Parse<SceneType>(v.SceneType);
            var scene = await SceneFactory.CreateStartScene(Root.Instance.Scene, id, instanceId, v.Zone, name, sceneType, v);
            scenes.Add(scene);
        }
        return scenes;
    }
}
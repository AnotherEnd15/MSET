using System;
using System.Collections.Generic;

namespace ET;

public class EtcdSceneConfig
{
    public long InstanceId;
    public string Ip;
    public SceneType SceneType;
    public int OuterPort; // Gate专用 暴露给客户端的端口
    public int Zone;
}

public class SceneEtcdValue
{
    public string Ip;
    public int OuterPort;
}

public class StartSceneService
{
    [StaticField]
    public static StartSceneService Instance = new StartSceneService();
    
    public Dictionary<long, EtcdSceneConfig> AllConfigs = new Dictionary<long, EtcdSceneConfig>();

    public Dictionary<(int zone, SceneType sceneType), List<EtcdSceneConfig>> ZoneScenes = new();

    public void AddConfig(string key, string value)
    {
        var keyStrs = key.Split('/');
        var sceneValue = JsonHelper.FromJson<SceneEtcdValue>(value);
        var config = new EtcdSceneConfig()
        {
            InstanceId = long.Parse(keyStrs[^1]),
            SceneType = Enum.Parse<SceneType>(keyStrs[^2]),
            Zone = int.Parse(keyStrs[^3]),
            Ip = sceneValue.Ip,
            OuterPort = sceneValue.OuterPort,
        };
        if (this.AllConfigs.ContainsKey(config.InstanceId))
        {
            return;
        }

        this.AllConfigs[config.InstanceId] = config;

        var zoneKey = (config.Zone, config.SceneType);
        if (!ZoneScenes.ContainsKey(zoneKey))
            ZoneScenes[zoneKey] = new();
        ZoneScenes[zoneKey].Add(config);
    }

    public void RemoveConfig(string key)
    {
        var keyStrs = key.Split('/');
        var instanceId = long.Parse(keyStrs[^1]);
        if (!this.AllConfigs.TryGetValue(instanceId, out var config))
            return;
        this.AllConfigs.Remove(instanceId);
        var zoneKey = (config.Zone, config.SceneType);
        this.ZoneScenes[zoneKey].Remove(config);
    }
}
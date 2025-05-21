using System.Collections.Generic;

namespace ET;

public class StartConfig
{
    public Dictionary<int, StartProcessConfig> ProcessConfigs = new();
    public List<StartSceneConfig> SceneConfigs = new();
}

public class StartSceneConfig
{
    public int Process;
    public string SceneType;
    public int OuterPort;
    public int Zone;
    public List<string> Tags = new();
}

public class StartProcessConfig
{
    public int Process;
    public string ip;
}


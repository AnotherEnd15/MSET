using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace ET;

public class ServerConfig
{
    [StaticField]
    public static ServerConfig Instance;

    public ServerConfig(string yamlStr, StartConfig startConfig)
    {
        this.StartConfig = startConfig;
        var input = new StringReader(yamlStr);
        var yamlStream = new YamlStream();
        yamlStream.Load(input);
        Config = yamlStream.Documents[0].RootNode;
        HostName = Config["host_namespace"].ToString();
        if (HostName == "{user_name}")
        {
            HostName = $"local_{Environment.UserName}";
        }
    }

    public YamlNode Config { get; set; }
    public string HostName { get; private set; }
    public StartConfig StartConfig { get; private set; }
}
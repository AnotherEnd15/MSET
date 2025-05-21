using System;
using System.IO;
using ET.Define;
using Luban;

namespace ET
{
    [ConsoleHandler(ConsoleMode.ReloadConfig)]
    public class ReloadConfigConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(ModeContex contex, string content)
        {
            switch (content)
            {
                case ConsoleMode.ReloadConfig:
                    contex.Parent.RemoveComponent<ModeContex>();
                    Log.GetLogger().Debug("C must have config name, like: C UnitConfig");
                    break;
                default:
                    string[] ss = content.Split(" ");
                    string configName = ss[1];
                    string category = $"{configName}Category";
                    Type type = EventSystem.Instance.GetType($"ET.{category}");
                    if (type == null)
                    {
                        Log.GetLogger().Debug($"reload config but not find {category}");
                        return;
                    }

                    var configFile = Path.Combine(PathConst.GetConfigPath(), category, ".bytes");
                    ConfigComponent.Instance.LoadOne(type,new ByteBuf(File.ReadAllBytes(configFile)));
                    Log.GetLogger().Debug($"reload config {configName} finish!");
                    break;
            }
            
            await ETTask.CompletedTask;
        }
    }
}
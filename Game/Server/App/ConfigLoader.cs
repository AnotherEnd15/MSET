using System;
using System.Collections.Generic;
using System.IO;

namespace ET.Server
{
    [Invoke]
    public class GetAllConfigBytes: AInvokeHandler<ConfigComponent.GetAllConfigBytes, ConfigResult>
    {
        public override ConfigResult Handle(ConfigComponent.GetAllConfigBytes args)
        {
            var result = new ConfigResult();

            var configFolder = "../Config/Excel";
            foreach (var v in Directory.GetFiles(configFolder,"*.bytes"))
            {
                result.Configs[Path.GetFileNameWithoutExtension(v)] = File.ReadAllBytes(v);
            }
            return result;
        }
    }
}
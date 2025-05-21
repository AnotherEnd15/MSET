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

            var configFolder = "../Unity/Assets/Bundles/Config";
            foreach (var v in Directory.GetFiles(configFolder,"*.bytes"))
            {
                result.Configs[Path.GetFileNameWithoutExtension(v)] = File.ReadAllBytes(v);
            }
            
            var spellConfigFolder = $"../Unity/Assets/Bundles/BattleLogic/Spell";
            var buffConfigFolder = $"../Unity/Assets/Bundles/BattleLogic/Buff";
            foreach (var v in Directory.GetFiles(spellConfigFolder,"*.bytes"))
            {
                result.SpellConfigs[Path.GetFileNameWithoutExtension(v)] = File.ReadAllBytes(v);
            }
            foreach (var v in Directory.GetFiles(buffConfigFolder,"*.bytes"))
            {
                result.BuffConfigs[Path.GetFileNameWithoutExtension(v)] = File.ReadAllBytes(v);
            }
            var mineMapConfigFolder = $"../Unity/Assets/Bundles/MineMap";
            foreach (var v in Directory.GetFiles(mineMapConfigFolder, "*.bytes"))
            {
                result.MineMapConfigs[Path.GetFileNameWithoutExtension(v)] = File.ReadAllBytes(v);
            }

            var bindPointPath = $"../Unity/Assets/Bundles/BattleLogic/BindPoint/BindPoint.bytes";
            result.BindPointConfig = File.ReadAllBytes(bindPointPath);
            

            return result;
        }
    }
}
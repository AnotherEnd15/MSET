#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ET
{
    public static class EditorTool
    {
        public static ConfigResult BuildConfigResult()
        {
            ConfigResult result = new();
            var files = Directory.GetFiles(Application.dataPath + "/Bundles/Config", "*.bytes");
         
            foreach (var v in files)
            {
                result.Configs[Path.GetFileNameWithoutExtension(v)] = File.ReadAllBytes(v);
            }
            
            return result;
        }
        
        public static void Combine()
        {
            var result = ET.EditorTool.BuildConfigResult();
            var resultPath = Application.dataPath + "/Bundles/CombinedConfigData.bytes";

            File.WriteAllBytes(resultPath,MemoryPackHelper.Serialize(result));
        }
    }
}
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MemoryPack;

namespace ET
{


    public class ConfigComponent: Singleton<ConfigComponent>
    {
        public List<IConfigCheck> ConfigChecks = new();
        
        public struct GetAllConfigBytes
        {
            
        }
    }
    
    [MemoryPackable]
    public partial class ConfigResult
    {
        [MemoryPackOrder(1)]
        public Dictionary<string, byte[]> Configs = new();
    }
}
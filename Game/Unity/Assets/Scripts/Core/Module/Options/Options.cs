#if !DOTNET
using System;
using System.Collections.Generic;

namespace ET
{
    public class Options: Singleton<Options>
    {
#if ROBOT
        [CommandLine.Option("Process", Required = false, Default = 1)]
        public int Process { get; set; }
        
        [CommandLine.Option("Develop", Required = false, Default = 0, HelpText = "develop mode, 0正式 1开发 2压测")]
        public int Develop { get; set; }

        [CommandLine.Option("LogLevel", Required = false, Default = 2)]
        public int LogLevel { get; set; }
        
        [CommandLine.Option("Console", Required = false, Default = 0)]
        public int Console { get; set; }
        
        [CommandLine.Option("TimeZone", Required = false, Default = 8)]
        public int TimeZone { get; set; }
        
#else
        public int Process { get; set; }

        public int TimeZone { get; set; }
#endif
        
    }
}
#endif
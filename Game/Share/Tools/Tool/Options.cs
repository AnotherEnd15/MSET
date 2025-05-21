

using CommandLine;
using System;
using System.Collections.Generic;

namespace ET
{

    public class Options
    {
        public static Options Instance;

        [Option("ActionType", Required = true)]
        public string ActionType { get; set; }

        [Option("Proto.ServerOutputPath", Required = false)]
        public string Proto_ServerOutputPath { get; set; }
        
        [Option("Proto.ClientOutputPath", Required = false)]
        public string Proto_ClientOutputPath { get; set; }
        
        [Option("InputPath", Required = false)]
        public string InputPath { get; set; }
        
        [Option("OutputPath", Required = false)]
        public string OutputPath { get; set; }
        
    }
}
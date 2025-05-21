
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using ILogger = Serilog.ILogger;

namespace ET
{

    public static class Log
    {
        public static void Init()
        {

        }

        [StaticField]
        private static readonly Dictionary<string, ILogger> Loggers = new();

        public static ILogger GetLogger([CallerFilePath] string callerFile = "")
        {
            if (Loggers.TryGetValue(callerFile, out var value))
            {
                return value;
            }
            
            Loggers[callerFile] = Serilog.Log.Logger.ForContext("SourceContext", $"({GetFileNameWithoutExtension(callerFile)})");
            return Loggers[callerFile];
        }

        static ILogger CreateLogger([CallerFilePath] string callerFile = "")
        {
            return Serilog.Log.Logger.ForContext("SourceContext", $"({GetFileNameWithoutExtension(callerFile)})");
        }
        
        public static void Error(this ILogger logger, Exception exception)
        {
            logger.Error(exception.ToString());
        }
        
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }
#if !DOTNET
        
        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            Serilog.Log.Logger.Debug(message);
        }
        
        public static void ImportantInfo(string message)
        {
            Serilog.Log.Logger.Information(message);
        }

        public static void Warning(string message)
        {
            Serilog.Log.Logger.Warning(message);
        }
        
        public static void Error(string exception)
        {
            Serilog.Log.Logger.Error(exception);
        }

        public static void Error(Exception exception)
        {
            Serilog.Log.Logger.Error(exception.ToString());
        }
        
#endif
    }
}
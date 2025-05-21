using Serilog;
using Serilog.Sinks.Unity3D;

namespace ET
{
    public static class LoggerConfigure
    {
        public static LoggerConfiguration BuildLogger()
        {
            var logger = new LoggerConfiguration();

            string loggerOutTemp = "[{Level:u3}] {SourceContext} {Message:lj} {NewLine}{Exception}";
#if UNITY_EDITOR
            logger.MinimumLevel.Debug()
                .Enrich.With<LogPropertyAdjust>()
                .WriteTo.Unity3D(outputTemplate: loggerOutTemp);
#else
            logger.MinimumLevel.Information()
                .WriteTo.Unity3D(outputTemplate: loggerOutTemp);
#endif
            
            
            
            return logger;
        }
    }
}
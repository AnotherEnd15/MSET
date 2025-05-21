using System;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace ET;

public class LoggerConfigure
{
    public static LoggerConfiguration Build()
    {
        var logger = new LoggerConfiguration();
        BuildNormal(logger);
        return logger;
    }

    public static void BuildNormal(LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.With<LogPropertyAdjust>()
                .Enrich.With<ErrorLogStack>()
                .WriteTo.Logger(c =>
                {
                    string loggerOutTemp = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " + $"(Process:{Options.Instance.Process})" +
                            " {SourceContext} {Message:lj} {NewLine}{Exception}{Stack}";
                    var fileName = $"../Logs/Process_{Options.Instance.Process:0000}_.log";

                    // 线上只打印至少info级别的日志
                    var minLogLevel = LogEventLevel.Debug;
#if !DEBUG
                    minLogLevel = LogEventLevel.Information;
#endif

                    c.WriteTo.File(fileName, minLogLevel, loggerOutTemp,
                        buffered: false, // 生产环境使用true,测试环境可以考虑使用false. (false代表日志每次打印都会立即写入文件)
                        rollingInterval: RollingInterval.Day, // 按天滚动日志文件,日志文件名后缀会带上日期(精确到天)
                        fileSizeLimitBytes: 30 * 1024 * 1024, // 30mb
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 30, // 该进程产生的日志文件,最终保留最新的30个
                        retainedFileTimeLimit: TimeSpan.FromDays(3) // 日志文件最终只保留最新3天的
                    ); // 日志文件最终只保留最新3天的);

                    if (Options.Instance != null && Options.Instance.Console == 1)
                        c.WriteTo.Console(LogEventLevel.Information, loggerOutTemp,theme: AnsiConsoleTheme.Code);
                });

    }
}
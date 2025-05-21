using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace ET;

public class ErrorLogStack : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Level < LogEventLevel.Error)
            return;
        // 已经有堆栈信息了,跳过
        if (logEvent.MessageTemplate.Text.Contains("Exception"))
            return;
        var stackTrace = new StackTrace(2,true);
        logEvent.AddPropertyIfAbsent(new LogEventProperty("Stack",new ScalarValue($"\n{stackTrace.ToString()}")));
    }
}
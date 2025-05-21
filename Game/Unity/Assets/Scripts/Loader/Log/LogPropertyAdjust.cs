using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace ET
{

    public class LogPropertyAdjust : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.Count == 0)
                return;
            foreach (var v in logEvent.Properties.Keys.ToList())
            {
                if (v == "SourceContext")
                    continue;
                var originValue = logEvent.Properties[v];
                if (Visit(v, originValue, out var newValue))
                {
                    logEvent.AddOrUpdateProperty(new LogEventProperty(v, newValue));
                }
            }
        }

        private bool Visit(string name, LogEventPropertyValue originValue, out LogEventPropertyValue newValue)
        {
            newValue = originValue;
            switch (originValue)
            {
                case ScalarValue scalar:
                    newValue = new ScalarValue($"{name}:{scalar.Value.ToString()}");
                    return true;
                case SequenceValue sequence:
                    return false;
                case StructureValue scalar:
                    return false;
                case DictionaryValue dictionary:
                    return false;
                default:
                    return false;
            }
        }
    }
}
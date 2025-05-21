using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public static class ConsoleMode
    {
        public const string TestCase = "TestCase";
    }

    [ComponentOf(typeof(Scene))]
    public class ConsoleComponent: Entity, IAwake, ILoad
    {
        public CancellationTokenSource CancellationTokenSource;
        public Dictionary<string, IConsoleHandler> Handlers = new Dictionary<string, IConsoleHandler>();
    }
}
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class CurrencyComponent : Entity,IAwake
    {
        public Dictionary<CurrencyType, long> Values = new();
    }
}
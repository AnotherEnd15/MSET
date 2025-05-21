using System.Collections.Generic;

namespace ET.GamePlay
{
    [ComponentOf(typeof(Scene))]
    public class PlayerCountCacheComponent : Entity,IAwake
    {
        public Dictionary<int, int> ZoneCount = new();
        public long LastQueryTime;
    }
}
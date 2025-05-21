using System.Collections.Generic;

namespace ET.GamePlay
{
    [ComponentOf(typeof(Scene))]
    public class ServerSettingComponent : Entity,IAwake
    {
        public Dictionary<int, ServerSetting> AllServerSettings = new();

        public long CloseTime;
        public long CloseServerTimer;

        public bool WhiteListOpen;
    }
}
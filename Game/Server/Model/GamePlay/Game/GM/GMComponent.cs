using System.Collections.Generic;

namespace ET.GamePlay
{
    [ComponentOf(typeof(Scene))]
    public class GMComponent : Entity,IAwake,ILoad
    {
        [StaticField]
        public static GMComponent Instance;
        public Dictionary<string, AGMHandler> AllGMHandlers = new();

        public Dictionary<string, string> GMFuncDefineInfo = new(); // GM方法定义的信息
    }
}
using System;
using System.Collections.Generic;

namespace ET.Server
{
 
    [ComponentOf(typeof(Scene))]
    public class DBManagerComponent: Entity, IAwake, IDestroy
    {
        [StaticField]
        public static DBManagerComponent Instance;

        public Dictionary<int, DBComponent> DBComponents = new();

        public HashSet<Type> AllDBTransferCom = new();
        
        public HashSet<Type> AllCenterUnitCom = new();
    }
}
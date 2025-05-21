using System.Collections.Generic;

namespace ET;

[ComponentOf(typeof(Scene))]
public class DBVersionComponent : Entity,IAwake
{
    public Dictionary<DBVersionEnum, IDBVersionHandler> AllVersionHandlers = new Dictionary<DBVersionEnum, IDBVersionHandler>();
}
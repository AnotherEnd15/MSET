using System.Collections.Generic;
using System.Net;

namespace ET;

[ComponentOf(typeof(Scene))]
public class NetWebSocketComponent : Entity, IAwake<string>, IDestroy
{
    public int ServiceId;
}
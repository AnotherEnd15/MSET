using System.Collections.Generic;
using System.Net;

namespace ET.Server;

[ComponentOf(typeof(Scene))]
public class NetWebSocketComponent : Entity, IAwake<string>, IDestroy
{
    public int ServiceId;
}
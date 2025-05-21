using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ChildOf()]
    public sealed class UI: Entity,IAwake, IDestroy
    {
        public UILayerType LayerType;
        public string UIType;
    }
}
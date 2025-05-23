using System;
using System.Collections.Generic;
using ET;

namespace ET.GamePlay
{
    public abstract class AGMHandler 
    {
        protected abstract ETTask<string> Run(Player unit, string args);
    }
}
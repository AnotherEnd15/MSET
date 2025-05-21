using System;
using ET.GameObjectPool;
using UnityEngine;

namespace ET
{
    public static class GameObjectComponentSystem
    {
        [ObjectSystem]
        public class DestroySystem: DestroySystem<GameObjectComponent>
        {
            protected override void Destroy(GameObjectComponent self)
            {
                var poolCom = GameObjectCenterPoolComponent.Instance;
                foreach (var v in self.SpellCreateObjs)
                {
                    foreach (var go in v.Value)
                    {
                        poolCom.Recycle(go);
                    }
                }
                
                self.SpellCreateObjs.Clear();
                
                foreach (var v in self.BuffAttachObjs)
                {
                    foreach (var go in v.Value)
                    {
                        poolCom.Recycle(go);
                    }
                }
                
                self.BuffAttachObjs.Clear();
                
                if (self.GameObject != null)
                {
                    poolCom.Recycle(self.GameObject);
                    self.GameObject = null;
                }


                if (self.StunEffect != null)
                {
                    poolCom.Recycle(self.StunEffect);
                    self.StunEffect = null;
                }

            }
        }
        

    }
}
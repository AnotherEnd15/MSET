using System;

namespace ET
{
    public static partial class UIComponentSystem
    {
        public static async ETTask RemoveSelfUI(this Entity self)
        {
            if (self is UI selfUI)
            {
                await self.DomainScene().GetComponent<UIComponent>().Remove(selfUI.UIType);
                return;
            }

            var parentUi = self.GetParent<UI>();
            if (parentUi is not UI ui)
            {
                throw new Exception($"非UI上的组件不能调用RemoveParentUI扩展方法 {self.GetType().FullName} {self.Parent.GetType().FullName}");
            }
            await self.DomainScene().GetComponent<UIComponent>().Remove(ui.UIType);
        }

        public static async ETTask RemoveUI(this Entity self,string uiType)
        {
            await self.DomainScene().GetComponent<UIComponent>().Remove(uiType);
        }

        public static async ETTask Remove(this UIComponent self,string uiType)
        {
#if UNITY_EDITOR
            if (!UIEventComponent.Instance.UIEvents.ContainsKey(uiType))
            {
                throw new Exception($"不存在类型{uiType}的UI.");
            }
#endif
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, uiType.GetHashCode()))
            {

                if (!self.UIs.TryGetValue(uiType, out ET.UI ui))
                {
                    return;
                }
                ui.Dispose();
            }
        }
    }
}
namespace ET
{
    public static class UISystem
    {
        public class UIDestroySystem : DestroySystem<UI>
        {
            protected override void Destroy(UI self)
            {
                if (string.IsNullOrEmpty(self.UIType))
                    return;
                // 专门处理子UI的销毁
                var uiCom = self.DomainScene().GetComponent<UIComponent>();
                uiCom?.UIs.Remove(self.UIType);
                UIEventComponent.Instance.OnRemove(self.UIType,self.DomainScene());
            }
        }
    }
}
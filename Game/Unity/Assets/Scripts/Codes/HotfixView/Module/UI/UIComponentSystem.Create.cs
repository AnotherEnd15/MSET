using System;

namespace ET
{
	public static partial class UIComponentSystem
	{
		public static async ETTask<UI> Create(this UIComponent self,string uiType, UILayerType layerType = UILayerType.Low)
		
		{
#if UNITY_EDITOR
			if (!UIEventComponent.Instance.UIEvents.ContainsKey(uiType))
			{
				throw new Exception($"不存在类型{uiType}的UI.");
			}
#endif

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, uiType.GetHashCode()))
			{
				var ui = self.Get(uiType);
				if (ui != null)
					return ui;

				if (self.IsDisposed)
					return null;


				ui = await UIEventComponent.Instance.OnCreate(uiType, self.Root, layerType);
				if (self.IsDisposed)
					return null;
				ui.LayerType = layerType;
				ui.UIType = uiType;
				self.UIs.Add(uiType, ui);

				if (!UIComponent.UIWithoutStack.Contains(uiType))
				{
					self.UIStack.Push(uiType);
				}

				return ui;

			}
		}
		
		public static async ETTask<UI> CreateWithParent(this UIComponent self,string childrenUiType, UI parent)
		{
#if UNITY_EDITOR
			if (!UIEventComponent.Instance.UIEvents.ContainsKey(childrenUiType))
			{
				throw new Exception($"不存在类型{childrenUiType}的UI.");
			}
#endif

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, childrenUiType.GetHashCode()))
			{
				var ui = self.Get(childrenUiType);
				if (ui != null)
					return ui;

				if (self.IsDisposed)
					return null;
				{
					ui = await UIEventComponent.Instance.OnCreate(childrenUiType,parent, parent.LayerType);
					if (self.IsDisposed)
						return null;
					ui.LayerType = parent.LayerType;
					ui.UIType = childrenUiType;
					self.UIs.Add(childrenUiType, ui);
					return ui;
				}
			}
		}


		public static async ETTask<UI> CreateUI(this Entity self,string uiType)
		{
			var parentUi = self.GetParent<UI>();
			if (parentUi is not UI ui)
			{
				throw new Exception("非UI上的组件不能调用CreateUI扩展方法");
			}

			return await ui.DomainScene().GetComponent<UIComponent>().CreateWithParent(uiType,ui);
		}
	}
}
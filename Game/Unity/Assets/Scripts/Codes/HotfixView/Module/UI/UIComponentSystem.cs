using System;
using System.Collections.Generic;
using ET;
using UnityEngine;

namespace ET
{
	
    /// <summary>
    /// 管理Scene上的UI
    /// </summary>
    [FriendOf(typeof (UIComponent))]
	public static partial class UIComponentSystem
	{
        public class AwakeSystem : AwakeSystem<UIComponent>
		{
			protected override void Awake(UIComponent self)
            {
                Log.Debug("UIComponentAwakeSystem");
                self.Root = self.AddChild<UI>();
            }
        }
        public class DestroySystem : DestroySystem<UIComponent>
        {
            protected override void Destroy(UIComponent self)
            {
				//self.SourceDirectorEffBox.Destroy();
				//self.effPos?.Dispose();
            }
        }


        public static UI GetUI(this Scene self,string uiType)
		{
			return self.GetComponent<UIComponent>().Get(uiType);
		}

		public static UI Get(this UIComponent self, string uiType)
		{
#if UNITY_EDITOR
			if (!UIEventComponent.Instance.UIEvents.ContainsKey(uiType))
			{
				throw new Exception($"不存在类型{uiType}的UI.");
			}
#endif
			ET.UI ui = null;
			self.UIs.TryGetValue(uiType, out ui);
			return ui;
		}

		public static async ETTask<ET.UI> Wait(this UIComponent self,string uiType,long MaxWaitTime=0)
		{
			long waitTime = 0;
			self.UIs.TryGetValue(uiType, out var ui);
			while (ui == null&&(MaxWaitTime==0||waitTime<=MaxWaitTime))
			{
				await TimerComponent.Instance.WaitAsync(100);
				waitTime += 100;
				self.UIs.TryGetValue(uiType, out ui);
			}
			return ui;
		}

		// 获取栈最顶层的有效的UI
		public static UI GetTopValidUIFromStack(this UIComponent self)
		{
			if (self.UIStack.Count == 0)
				return null;
			while (self.UIStack.Count>0)
			{
				var uiType = self.UIStack.Peek();
				if (self.UIs.TryGetValue(uiType, out var ui))
				{
					return ui;
				}

				self.UIStack.Pop();
			}

			return null;
		}

		// 关闭栈中所有UI
		public static async ETTask CloseAllUIInStack(this UIComponent self)
		{
			if (self.UIStack.Count == 0)
				return;
            List<ETTask> tasks = new List<ETTask>();
            while (self.UIStack.Count>0)
			{
				var uiType = self.UIStack.Pop();
				if (!self.UIs.TryGetValue(uiType, out var ui))
				{
					continue;
				}
				tasks.Add(self.Remove(uiType));
			}
			await ETTaskHelper.WaitAll(tasks);
		}
		//public static EffBox GetEffPos(this UIComponent self)
		//{
		//	if (self.effPos == null)
		//	{
		//		self.effPos = EffBox.CreateInstance();
		//		self.effPos.title = ConstValue.SourceDirectorEff;
		//		self.effPos.onClick.Set(() => self.effPos.visible = false);
		//		GRoot.inst.AddChild(self.effPos);
		//	}
		//	return self.effPos;
        //}
		public static List<UI> GetUIListByLayer(this UIComponent self, UILayerType layer)
		{
			List<UI> uIs = new List<UI>();
			foreach (var ui in self.UIs.Values)
			{
                if (ui.LayerType == layer)
				{
                    uIs.Add(ui);
                }
            }
			return uIs;
		}

    }
}
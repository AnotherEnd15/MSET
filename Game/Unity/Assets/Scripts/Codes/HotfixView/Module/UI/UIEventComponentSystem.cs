using System;
using ET;
using UnityEngine;

namespace ET
{
	/// <summary>
	/// 管理所有UI GameObject 以及UI事件
	/// </summary>
	[FriendOf(typeof(UIEventComponent))]
	public static class UIEventComponentSystem
	{
		[ObjectSystem]
		public class UIEventComponentAwakeSystem : AwakeSystem<UIEventComponent>
		{
			protected override void Awake(UIEventComponent self)
			{
				UIEventComponent.Instance = self;
				
				var uiEvents = EventSystem.Instance.GetTypes(typeof (UIEventAttribute));
				foreach (Type type in uiEvents)
				{
					object[] attrs = type.GetCustomAttributes(typeof(UIEventAttribute), false);
					if (attrs.Length == 0)
					{
						continue;
					}

					UIEventAttribute uiEventAttribute = attrs[0] as UIEventAttribute;
					AUIEvent aUIEvent = Activator.CreateInstance(type) as AUIEvent;
					self.UIEvents.Add(uiEventAttribute.UIType, aUIEvent);
				}
			}
		}

		public static async ETTask<ET.UI> OnCreate(this UIEventComponent self,string uiType, UI parentUI,UILayerType layerType)
		{
			try
			{
				ET.UI ui = await self.UIEvents[uiType].OnCreate(parentUI,layerType);
				return ui;
			}
			catch (Exception e)
			{
				throw new Exception($"on create ui error: {uiType}", e);
			}
		}
		
		
		public static void OnRemove(this UIEventComponent self,string uiType,Scene scene)
		{
			try
			{
				self.UIEvents[uiType].OnRemove(scene);
			}
			catch (Exception e)
			{
				throw new Exception($"on remove ui error: {uiType}", e);
			}
			
		}
	}
}
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ET
{
	public static class GameObjectHelper
	{
		public static T Get<T>(this GameObject gameObject, string key) where T : class
		{
			try
			{
				return gameObject.GetComponent<ReferenceCollector>().Get<T>(key);
			}
			catch (Exception e)
			{
				throw new Exception($"获取{gameObject.name}的ReferenceCollector key失败, key: {key}", e);
			}
		}
		public static void SetOrderLayer(this SortingGroup self,float posY)
		{
			self.sortingOrder = (int)(10000-posY * 100);
		}
	}
}
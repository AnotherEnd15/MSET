using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
	/// <summary>
	/// 管理所有UI GameObject
	/// </summary>
	[ComponentOf(typeof(Scene))]
	public class UIEventComponent: Entity, IAwake
	{
		[StaticField]
		public static UIEventComponent Instance;
		
		public Dictionary<string, AUIEvent> UIEvents = new ();
	}
}
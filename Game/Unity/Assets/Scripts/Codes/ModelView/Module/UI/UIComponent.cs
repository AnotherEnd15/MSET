using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
	/// <summary>
	/// 管理Scene上的UI
	/// </summary>
	[ComponentOf(typeof(Scene))]
	public class UIComponent: Entity, IAwake,IDestroy
	{
		public UI Root; // 空UI
		public Dictionary<string, UI> UIs = new ();

		// 使用栈结构额外管理的UI 方便返回上一层UI/出现引导时关闭所有非固定UI等操作
		public Stack<string> UIStack = new();
		
		// 不使用栈式结构的UI 基本是某些固定的界面
		[StaticField] public static HashSet<string> UIWithoutStack = new()
		{
			
        };

		#region 来源表指引特效
		//public EffBox effPos;
		//public UIEffBox SourceDirectorEffBox = new UIEffBox ();
		public UI UI_Effect_Uppermost;
        #endregion
    }

}
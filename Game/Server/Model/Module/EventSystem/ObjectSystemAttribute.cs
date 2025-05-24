using System;

namespace ET
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class ObjectSystemAttribute: BaseAttribute
	{
	}
}
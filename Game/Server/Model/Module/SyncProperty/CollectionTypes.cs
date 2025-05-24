using System;

namespace SyncFramework
{
	// 集合操作类型
	public enum CollectionOperation : byte
	{
		Add = 1,
		Remove = 2,
		Clear = 3,
		Replace = 4
	}

	// 集合变更记录
	public class CollectionChange
	{
		public CollectionOperation Operation { get; set; }
		public object Key { get; set; }     // Dict的key或List的index
		public object Value { get; set; }   // 值
		public object OldValue { get; set; } // 替换操作的旧值
	}
} 
using System;
using System.Collections.Generic;

namespace SyncFramework
{
	/// <summary>
	/// SyncFramework 使用示例
	/// 展示 ISyncCollection 接口的正确使用方式
	/// </summary>
	public static class SyncUsageExample
	{
		public static void ExampleUsage()
		{
			// 1. 创建Player对象
			var player = new ET.Player();
			
			// 2. 初始化同步集合 - 这里会自动设置 ISyncCollection 的回调
			player.SyncNameList = new SyncList<string>();
			player.SyncStats = new SyncDictionary<string, int>();
			
			// 3. 操作集合 - 这些操作会被自动追踪
			player.SyncNameList.Add("张三");
			player.SyncNameList.Add("李四");
			player.SyncStats["HP"] = 100;
			player.SyncStats["MP"] = 50;
			
			// 4. 生成同步消息 - 会包含所有变更
			byte[] syncMessage = SyncHelper.CreateSyncMessage(player);
			
			Console.WriteLine($"同步消息大小: {syncMessage.Length} bytes");
			Console.WriteLine($"脏数据字段数量: {player.DirtyCount}");
			
			// 5. 应用到另一个对象
			var otherPlayer = new ET.Player();
			SyncHelper.DeserializeObject(otherPlayer, syncMessage);
		}
		
		/// <summary>
		/// 演示手动设置ISyncCollection回调的方式（生成的代码会自动处理）
		/// </summary>
		public static void ManualCallbackExample()
		{
			var syncList = new SyncList<string>();
			
			// 手动设置回调（生成的代码会自动调用这个方法）
			syncList.SetChangeCallback(123, (fieldId, change) =>
			{
				Console.WriteLine($"字段 {fieldId} 发生变更: {change.Operation}");
				switch (change.Operation)
				{
					case CollectionOperation.Add:
						Console.WriteLine($"  添加: {change.Value} at {change.Key}");
						break;
					case CollectionOperation.Remove:
						Console.WriteLine($"  删除: {change.Value} at {change.Key}");
						break;
					case CollectionOperation.Replace:
						Console.WriteLine($"  替换: {change.OldValue} -> {change.Value} at {change.Key}");
						break;
					case CollectionOperation.Clear:
						Console.WriteLine($"  清空集合");
						break;
				}
			});
			
			// 测试操作
			syncList.Add("Item 1");
			syncList.Add("Item 2");
			syncList[0] = "Modified Item 1";
			syncList.RemoveAt(1);
			syncList.Clear();
		}
		
		/// <summary>
		/// 演示不同集合类型的ISyncCollection使用
		/// </summary>
		public static void CollectionTypesExample()
		{
			// SyncList<T> - 实现 ISyncCollection
			var syncList = new SyncList<int>();
			ISyncCollection collection1 = syncList;
			collection1.SetChangeCallback(1, LogChange);
			
			// SyncDictionary<K,V> - 实现 ISyncCollection  
			var syncDict = new SyncDictionary<string, float>();
			ISyncCollection collection2 = syncDict;
			collection2.SetChangeCallback(2, LogChange);
			
			// 测试操作
			syncList.Add(100);
			syncDict["score"] = 95.5f;
		}
		
		private static void LogChange(int fieldId, CollectionChange change)
		{
			Console.WriteLine($"[Field {fieldId}] {change.Operation}: {change.Key} = {change.Value}");
		}
	}
} 
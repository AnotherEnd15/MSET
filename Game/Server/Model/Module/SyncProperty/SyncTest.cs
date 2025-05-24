using System;
using System.Collections.Generic;
using SyncFramework;
using ET;
using System.Linq;

namespace SyncFramework
{
	/// <summary>
	/// 同步框架测试类
	/// 测试各种同步场景：基本类型、普通集合、同步集合、复合场景
	/// </summary>
	public static class SyncTest
	{
		public static void RunAllTests()
		{
			try
			{
				Console.WriteLine("=== 开始同步框架测试 ===");
				
				TestPrimitiveSync();
				TestCollectionSync();
				TestSyncCollectionSync();
				TestComplexSync();
				
				Console.WriteLine("\n=== 所有测试通过! ===");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\n=== 测试失败: {ex.Message} ===");
				throw;
			}
		}

		/// <summary>
		/// 测试基本类型同步（string, int等）
		/// </summary>
		public static void TestPrimitiveSync()
		{
			Console.WriteLine("\n--- 测试基本类型同步 ---");
			
			// 创建Player A并修改基本属性
			var playerA = new Player();
			playerA.Name = "TestPlayer";
			
			Console.WriteLine($"PlayerA Name: {playerA.Name}");
			Console.WriteLine($"PlayerA IsDirty: {playerA.IsDirty}");
			
			// 序列化A的脏数据
			var data = SyncHelper.CreateSyncMessage(playerA);
			Console.WriteLine($"序列化数据长度: {data.Length} bytes");
			
			// 创建Player B并应用数据
			var playerB = new Player();
			SyncHelper.DeserializeObject(playerB, data);
			
			Console.WriteLine($"PlayerB Name: {playerB.Name}");
			
			// 验证同步结果
			if (playerA.Name == playerB.Name)
			{
				Console.WriteLine("✓ 基本类型同步成功");
			}
			else
			{
				throw new Exception($"基本类型同步失败: A={playerA.Name}, B={playerB.Name}");
			}
		}

		/// <summary>
		/// 测试普通集合同步（List, Dictionary）
		/// </summary>
		public static void TestCollectionSync()
		{
			Console.WriteLine("\n--- 测试普通集合同步 ---");
			
			// 创建Player A并设置集合（普通集合只支持整体替换）
			var playerA = new Player();
			Console.WriteLine($"PlayerA 初始状态 - IsDirty: {playerA.IsDirty}, DirtyCount: {playerA.DirtyCount}");
			
			playerA.NameList = new List<string> { "item1", "item2", "item3" };
			Console.WriteLine($"PlayerA 设置NameList后 - IsDirty: {playerA.IsDirty}, DirtyCount: {playerA.DirtyCount}");
			
			playerA.NameDict = new Dictionary<string, string> 
			{ 
				{ "key1", "value1" }, 
				{ "key2", "value2" },
				{ "key3", "value3" }
			};
			Console.WriteLine($"PlayerA 设置NameDict后 - IsDirty: {playerA.IsDirty}, DirtyCount: {playerA.DirtyCount}");
			
			Console.WriteLine($"PlayerA NameList count: {playerA.NameList?.Count}");
			Console.WriteLine($"PlayerA NameDict count: {playerA.NameDict?.Count}");
			
			// 序列化A的脏数据
			var data = SyncHelper.CreateSyncMessage(playerA);
			Console.WriteLine($"序列化数据长度: {data.Length} bytes");
			
			if (data.Length > 0)
			{
				// 显示前20个字节的十六进制
				var hexBytes = string.Join(" ", data.Take(Math.Min(20, data.Length)).Select(b => b.ToString("X2")));
				Console.WriteLine($"序列化数据前{Math.Min(20, data.Length)}字节: {hexBytes}");
			}
			
			if (data.Length == 0)
			{
				Console.WriteLine("⚠ 警告: 序列化数据为空！可能DirtyCount为0");
				return;
			}
			
			// 创建Player B并应用数据
			var playerB = new Player();
			
			Console.WriteLine($"PlayerB 反序列化前 - NameList: {playerB.NameList?.Count}, NameDict: {playerB.NameDict?.Count}");
			
			try
			{
				SyncHelper.DeserializeObject(playerB, data);
				Console.WriteLine("反序列化成功");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"反序列化失败: {ex.Message}");
				Console.WriteLine($"StackTrace: {ex.StackTrace}");
				throw;
			}
			
			Console.WriteLine($"PlayerB 反序列化后 - NameList: {playerB.NameList?.Count}, NameDict: {playerB.NameDict?.Count}");
			
			// 详细验证NameDict内容
			if (playerB.NameDict != null)
			{
				Console.WriteLine($"PlayerB NameDict详细内容:");
				foreach (var kvp in playerB.NameDict)
				{
					Console.WriteLine($"  {kvp.Key} => {kvp.Value}");
				}
			}
			
			// 验证同步结果
			if (playerB.NameList?.Count == 3 && playerB.NameDict?.Count == 3)
			{
				Console.WriteLine("✓ 普通集合同步成功");
			}
			else
			{
				Console.WriteLine($"✗ 普通集合同步失败:");
				Console.WriteLine($"  期望: NameList=3, NameDict=3");
				Console.WriteLine($"  实际: NameList={playerB.NameList?.Count}, NameDict={playerB.NameDict?.Count}");
				throw new Exception($"普通集合同步失败: NameList={playerB.NameList?.Count}, NameDict={playerB.NameDict?.Count}");
			}
		}

		/// <summary>
		/// 测试同步集合（SyncList, SyncDictionary）
		/// </summary>
		public static void TestSyncCollectionSync()
		{
			Console.WriteLine("\n--- 测试同步集合 ---");
			
			// 创建Player A并修改同步集合
			var playerA = new Player();
			playerA.SyncNameList = new SyncList<string>();
			playerA.SyncStats = new SyncDictionary<string, int>();
			
			// 直接使用SyncList/SyncDictionary的方法（应该自动触发回调）
			playerA.SyncNameList.Add("sync1");
			playerA.SyncNameList.Add("sync2");
			playerA.SyncStats.Add("hp", 100);
			playerA.SyncStats.Add("mp", 50);
			
			// 也测试生成的辅助方法
			playerA.AddSyncNameList("sync3");
			playerA.SetSyncStats("level", 10);
			
			Console.WriteLine($"PlayerA SyncNameList count: {playerA.SyncNameList?.Count}");
			Console.WriteLine($"PlayerA SyncStats count: {playerA.SyncStats?.Count}");
			Console.WriteLine($"PlayerA IsDirty: {playerA.IsDirty}");
			
			// 序列化A的脏数据
			var data = SyncHelper.CreateSyncMessage(playerA);
			Console.WriteLine($"序列化数据长度: {data.Length} bytes");
			
			// 创建Player B并应用数据
			var playerB = new Player();
			SyncHelper.DeserializeObject(playerB, data);
			
			Console.WriteLine($"PlayerB SyncNameList count: {playerB.SyncNameList?.Count}");
			Console.WriteLine($"PlayerB SyncStats count: {playerB.SyncStats?.Count}");
			
			// 验证同步结果
			if (playerB.SyncNameList?.Count == 3 && playerB.SyncStats?.Count == 3)
			{
				Console.WriteLine("✓ 同步集合同步成功");
				
				// 测试反序列化后的回调是否正确设置
				Console.WriteLine("\n测试反序列化后的回调设置...");
				var oldDirtyCount = playerB.DirtyCount;
				playerB.SyncNameList?.Add("test_callback");
				
				if (playerB.DirtyCount > oldDirtyCount)
				{
					Console.WriteLine("✓ 反序列化后回调设置正确");
				}
				else
				{
					Console.WriteLine("⚠ 反序列化后回调可能未正确设置");
				}
			}
			else
			{
				throw new Exception($"同步集合同步失败: SyncNameList={playerB.SyncNameList?.Count}, SyncStats={playerB.SyncStats?.Count}");
			}
		}

		/// <summary>
		/// 测试复合场景：多种类型混合修改
		/// </summary>
		public static void TestComplexSync()
		{
			Console.WriteLine("\n--- 测试复合场景 ---");
			
			// 创建Player A并进行复合修改
			var playerA = new Player();
			
			// 修改基本类型
			playerA.Name = "ComplexPlayer";
			
			// 修改普通集合（整体替换）
			playerA.NameList = new List<string> { "a", "b", "c" };
			
			// 修改同步集合
			playerA.SyncNameList = new SyncList<string>();
			playerA.SyncNameList.Add("sync_a");
			playerA.AddSyncNameList("sync_b");
			
			playerA.SyncStats = new SyncDictionary<string, int>();
			playerA.SyncStats["health"] = 200;
			playerA.SetSyncStats("mana", 150);
			
			Console.WriteLine($"PlayerA 修改完成，脏字段数: {playerA.DirtyCount}");
			
			// 序列化
			var data = SyncHelper.CreateSyncMessage(playerA);
			Console.WriteLine($"序列化数据长度: {data.Length} bytes");
			
			// 应用到Player B
			var playerB = new Player();
			SyncHelper.DeserializeObject(playerB, data);
			
			// 全面验证
			bool success = 
				playerB.Name == "ComplexPlayer" &&
				playerB.NameList?.Count == 3 &&
				playerB.SyncNameList?.Count == 2 &&
				playerB.SyncStats?.Count == 2 &&
				playerB.SyncStats?.ContainsKey("health") == true &&
				playerB.SyncStats?["health"] == 200;
			
			if (success)
			{
				Console.WriteLine("✓ 复合场景同步成功");
				
				// 验证PlayerB可以继续进行同步操作
				playerB.SyncNameList?.Add("test_continue");
				if (playerB.IsDirty)
				{
					Console.WriteLine("✓ PlayerB可以继续同步操作");
				}
			}
			else
			{
				throw new Exception("复合场景同步失败");
			}
		}

		/// <summary>
		/// 辅助方法：打印Player状态
		/// </summary>
		public static void PrintPlayerState(Player player, string prefix = "")
		{
			Console.WriteLine($"{prefix}Player状态:");
			Console.WriteLine($"  Name: {player.Name}");
			Console.WriteLine($"  NameList: {player.NameList?.Count} items");
			Console.WriteLine($"  NameDict: {player.NameDict?.Count} items");
			Console.WriteLine($"  SyncNameList: {player.SyncNameList?.Count} items");
			Console.WriteLine($"  SyncStats: {player.SyncStats?.Count} items");
			Console.WriteLine($"  IsDirty: {player.IsDirty}");
			Console.WriteLine($"  DirtyCount: {player.DirtyCount}");
		}
	}
} 
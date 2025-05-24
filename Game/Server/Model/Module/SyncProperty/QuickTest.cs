using System;
using ET;
using SyncFramework;

namespace SyncFramework
{
	public static class QuickTest
	{
		public static void TestBasicSync()
		{
			Console.WriteLine("=== 快速测试基本同步 ===");
			
			// 创建Player A
			var playerA = new Player();
			Console.WriteLine($"PlayerA初始状态 - Name: '{playerA.Name}', IsDirty: {playerA.IsDirty}");
			
			// 修改Name
			playerA.Name = "TestPlayer";
			Console.WriteLine($"PlayerA设置Name后 - Name: '{playerA.Name}', IsDirty: {playerA.IsDirty}, DirtyCount: {playerA.DirtyCount}");
			
			// 序列化
			var data = SyncHelper.CreateSyncMessage(playerA);
			Console.WriteLine($"序列化数据长度: {data.Length} bytes");
			
			if (data.Length == 0)
			{
				Console.WriteLine("❌ 错误：序列化数据为空");
				return;
			}
			
			// 创建Player B
			var playerB = new Player();
			Console.WriteLine($"PlayerB初始状态 - Name: '{playerB.Name}'");
			
			// 反序列化
			try
			{
				SyncHelper.DeserializeObject(playerB, data);
				Console.WriteLine($"PlayerB反序列化后 - Name: '{playerB.Name}'");
				
				if (playerA.Name == playerB.Name)
				{
					Console.WriteLine("✅ 基本同步测试成功！");
				}
				else
				{
					Console.WriteLine($"❌ 同步失败: A='{playerA.Name}', B='{playerB.Name}'");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ 反序列化出错: {ex.Message}");
				Console.WriteLine(ex.StackTrace);
			}
		}
	}
} 
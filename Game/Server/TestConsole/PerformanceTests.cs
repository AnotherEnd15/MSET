using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using SyncFramework;
using System.Collections.Generic;

namespace TestConsole
{
    /// <summary>
    /// 性能测试类 - 测试同步框架的性能表现
    /// </summary>
    [TestClass]
    public class PerformanceTests
    {
        /// <summary>
        /// 测试大量数据的同步性能
        /// </summary>
        [TestMethod]
        public void TestLargeDataSyncPerformance()
        {
            Console.WriteLine("\n--- 测试大量数据同步性能 ---");
            
            var playerA = new TestPlayer();
            var stopwatch = new Stopwatch();
            
            // 准备大量数据
            Console.WriteLine("准备测试数据...");
            stopwatch.Start();
            
            // 大量普通集合数据
            playerA.NameList = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                playerA.NameList.Add($"item_{i}");
            }
            
            playerA.NameDict = new Dictionary<string, string>();
            for (int i = 0; i < 1000; i++)
            {
                playerA.NameDict[$"key_{i}"] = $"value_{i}";
            }
            
            // 大量同步集合数据
            playerA.SyncNameList = new SyncList<string>();
            for (int i = 0; i < 1000; i++)
            {
                playerA.SyncNameList.Add($"sync_item_{i}");
            }
            
            playerA.SyncStats = new SyncDictionary<string, int>();
            for (int i = 0; i < 1000; i++)
            {
                playerA.SyncStats[$"stat_{i}"] = i;
            }
            
            stopwatch.Stop();
            Console.WriteLine($"数据准备用时: {stopwatch.ElapsedMilliseconds}ms");
            
            // 测试序列化性能
            Console.WriteLine("测试序列化性能...");
            stopwatch.Restart();
            
            var data = SyncHelper.CreateSyncMessage(playerA);
            
            stopwatch.Stop();
            Console.WriteLine($"序列化用时: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"序列化数据大小: {data.Length} bytes");
            
            Assert.IsTrue(data.Length > 0, "序列化数据不应为空");
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, "序列化时间应在合理范围内");
            
            // 测试反序列化性能
            Console.WriteLine("测试反序列化性能...");
            var playerB = new TestPlayer();
            
            stopwatch.Restart();
            SyncHelper.DeserializeObject(playerB, data);
            stopwatch.Stop();
            
            Console.WriteLine($"反序列化用时: {stopwatch.ElapsedMilliseconds}ms");
            
            // 验证数据完整性
            Assert.AreEqual(1000, playerB.NameList?.Count, "NameList数据应完整");
            Assert.AreEqual(1000, playerB.NameDict?.Count, "NameDict数据应完整");
            Assert.AreEqual(1000, playerB.SyncNameList?.Count, "SyncNameList数据应完整");
            Assert.AreEqual(1000, playerB.SyncStats?.Count, "SyncStats数据应完整");
            
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, "反序列化时间应在合理范围内");
            
            Console.WriteLine("✓ 大量数据同步性能测试通过");
        }

        /// <summary>
        /// 测试频繁同步操作的性能
        /// </summary>
        [TestMethod]
        public void TestFrequentSyncPerformance()
        {
            Console.WriteLine("\n--- 测试频繁同步性能 ---");
            
            var playerA = new TestPlayer();
            var playerB = new TestPlayer();
            var stopwatch = new Stopwatch();
            
            // 初始化同步集合
            playerA.SyncNameList = new SyncList<string>();
            playerA.SyncStats = new SyncDictionary<string, int>();
            
            Console.WriteLine("测试频繁增量同步...");
            stopwatch.Start();
            
            const int iterations = 100;
            for (int i = 0; i < iterations; i++)
            {
                // 增量修改
                playerA.SyncNameList.Add($"item_{i}");
                playerA.SyncStats[$"stat_{i}"] = i;
                
                // 同步
                var data = SyncHelper.CreateSyncMessage(playerA);
                if (data.Length > 0)
                {
                    SyncHelper.DeserializeObject(playerB, data);
                }
            }
            
            stopwatch.Stop();
            Console.WriteLine($"完成 {iterations} 次增量同步，总用时: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"平均每次同步用时: {stopwatch.ElapsedMilliseconds / (double)iterations:F2}ms");
            
            // 验证最终状态
            Assert.AreEqual(iterations, playerB.SyncNameList?.Count, "SyncNameList应有正确数量的元素");
            Assert.AreEqual(iterations, playerB.SyncStats?.Count, "SyncStats应有正确数量的元素");
            
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10000, "频繁同步总时间应在合理范围内");
            
            Console.WriteLine("✓ 频繁同步性能测试通过");
        }

        /// <summary>
        /// 测试内存使用情况
        /// </summary>
        [TestMethod]
        public void TestMemoryUsage()
        {
            Console.WriteLine("\n--- 测试内存使用情况 ---");
            
            // 强制垃圾回收，获取基准内存
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var memoryBefore = GC.GetTotalMemory(false);
            Console.WriteLine($"基准内存使用: {memoryBefore / 1024.0:F2} KB");
            
            // 创建大量TestPlayer对象并进行同步
            const int playerCount = 100;
            var players = new List<TestPlayer>();
            
            for (int i = 0; i < playerCount; i++)
            {
                var player = new TestPlayer();
                player.Name = $"Player_{i}";
                player.Level = i;
                player.SyncNameList = new SyncList<string>();
                player.SyncStats = new SyncDictionary<string, int>();
                
                // 添加一些数据
                for (int j = 0; j < 10; j++)
                {
                    player.SyncNameList.Add($"item_{j}");
                    player.SyncStats[$"stat_{j}"] = j;
                }
                
                players.Add(player);
            }
            
            // 进行同步操作
            for (int i = 0; i < playerCount - 1; i++)
            {
                var data = SyncHelper.CreateSyncMessage(players[i]);
                if (data.Length > 0)
                {
                    SyncHelper.DeserializeObject(players[i + 1], data);
                }
            }
            
            var memoryAfter = GC.GetTotalMemory(false);
            var memoryUsed = memoryAfter - memoryBefore;
            Console.WriteLine($"操作后内存使用: {memoryAfter / 1024.0:F2} KB");
            Console.WriteLine($"额外内存使用: {memoryUsed / 1024.0:F2} KB");
            Console.WriteLine($"平均每个TestPlayer内存: {memoryUsed / (double)playerCount / 1024.0:F2} KB");
            
            // 强制垃圾回收
            players.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var memoryFinal = GC.GetTotalMemory(false);
            Console.WriteLine($"回收后内存使用: {memoryFinal / 1024.0:F2} KB");
            
            // 内存使用应该在合理范围内
            Assert.IsTrue(memoryUsed < 50 * 1024 * 1024, "内存使用应在合理范围内"); // 50MB
            
            Console.WriteLine("✓ 内存使用测试通过");
        }

        /// <summary>
        /// 压力测试：极限数据量
        /// </summary>
        [TestMethod]
        [Timeout(30000)] // 30秒超时
        public void TestStressWithExtremeData()
        {
            Console.WriteLine("\n--- 压力测试：极限数据量 ---");
            
            var playerA = new TestPlayer();
            var stopwatch = new Stopwatch();
            
            Console.WriteLine("创建极限数据量...");
            stopwatch.Start();
            
            // 极大的字符串
            playerA.Name = new string('A', 10000); // 10KB字符串
            playerA.Level = 999999;
            
            // 极大的普通集合
            playerA.NameList = new List<string>();
            for (int i = 0; i < 5000; i++)
            {
                playerA.NameList.Add($"very_long_item_name_for_stress_testing_{i}_{new string('X', 100)}");
            }
            
            // 极大的同步集合
            playerA.SyncNameList = new SyncList<string>();
            for (int i = 0; i < 2000; i++)
            {
                playerA.SyncNameList.Add($"sync_stress_item_{i}");
            }
            
            playerA.SyncStats = new SyncDictionary<string, int>();
            for (int i = 0; i < 2000; i++)
            {
                playerA.SyncStats[$"stress_stat_with_very_long_key_name_{i}"] = i;
            }
            
            stopwatch.Stop();
            Console.WriteLine($"数据创建用时: {stopwatch.ElapsedMilliseconds}ms");
            
            Console.WriteLine("进行极限数据同步...");
            stopwatch.Restart();
            
            var data = SyncHelper.CreateSyncMessage(playerA);
            
            stopwatch.Stop();
            Console.WriteLine($"极限数据序列化用时: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"极限数据大小: {data.Length / 1024.0:F2} KB");
            
            Assert.IsTrue(data.Length > 0, "极限数据序列化应成功");
            
            // 反序列化
            stopwatch.Restart();
            var playerB = new TestPlayer();
            SyncHelper.DeserializeObject(playerB, data);
            stopwatch.Stop();
            
            Console.WriteLine($"极限数据反序列化用时: {stopwatch.ElapsedMilliseconds}ms");
            
            // 验证数据完整性
            Assert.AreEqual(playerA.Name.Length, playerB.Name?.Length, "极限字符串应正确同步");
            Assert.AreEqual(999999, playerB.Level, "极限数值应正确同步");
            Assert.AreEqual(5000, playerB.NameList?.Count, "极限NameList应正确同步");
            Assert.AreEqual(2000, playerB.SyncNameList?.Count, "极限SyncNameList应正确同步");
            Assert.AreEqual(2000, playerB.SyncStats?.Count, "极限SyncStats应正确同步");
            
            Console.WriteLine("✓ 极限数据压力测试通过");
        }

        /// <summary>
        /// 测试不同实体类型的性能
        /// </summary>
        [TestMethod]
        public void TestDifferentEntityPerformance()
        {
            Console.WriteLine("\n--- 测试不同实体类型性能 ---");
            var stopwatch = new Stopwatch();
            
            // 测试SimpleEntity性能
            Console.WriteLine("测试SimpleEntity性能...");
            stopwatch.Start();
            
            const int simpleEntityCount = 1000;
            for (int i = 0; i < simpleEntityCount; i++)
            {
                var entityA = new SimpleEntity($"id_{i}", $"data_{i}");
                var entityB = new SimpleEntity();
                
                var data = SyncHelper.CreateSyncMessage(entityA);
                SyncHelper.DeserializeObject(entityB, data);
                
                Assert.AreEqual($"id_{i}", entityB.Id);
                Assert.AreEqual($"data_{i}", entityB.Data);
            }
            
            stopwatch.Stop();
            Console.WriteLine($"SimpleEntity {simpleEntityCount}次同步用时: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"平均每次用时: {stopwatch.ElapsedMilliseconds / (double)simpleEntityCount:F2}ms");
            
            // 测试CollectionEntity性能
            Console.WriteLine("测试CollectionEntity性能...");
            stopwatch.Restart();
            
            const int collectionEntityCount = 100;
            for (int i = 0; i < collectionEntityCount; i++)
            {
                var entityA = new CollectionEntity();
                entityA.Items.Add($"item_{i}");
                entityA.Scores[$"score_{i}"] = i;
                entityA.SyncItems.Add($"syncItem_{i}");
                entityA.SyncScores[$"syncScore_{i}"] = i * 10;
                
                var entityB = new CollectionEntity();
                
                var data = SyncHelper.CreateSyncMessage(entityA);
                SyncHelper.DeserializeObject(entityB, data);
                
                Assert.AreEqual(1, entityB.Items?.Count);
                Assert.AreEqual(1, entityB.Scores?.Count);
                Assert.AreEqual(1, entityB.SyncItems?.Count);
                Assert.AreEqual(1, entityB.SyncScores?.Count);
            }
            
            stopwatch.Stop();
            Console.WriteLine($"CollectionEntity {collectionEntityCount}次同步用时: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"平均每次用时: {stopwatch.ElapsedMilliseconds / (double)collectionEntityCount:F2}ms");
            
            Console.WriteLine("✓ 不同实体类型性能测试通过");
        }
    }
} 
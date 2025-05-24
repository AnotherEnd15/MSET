using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SyncFramework;
using System.Collections.Generic;

namespace TestConsole
{
    /// <summary>
    /// 复合场景测试类 - 测试混合类型的复杂同步场景
    /// </summary>
    [TestClass]
    public class ComplexSyncTests
    {
        /// <summary>
        /// 测试复合场景：多种类型混合修改
        /// </summary>
        [TestMethod]
        public void TestComplexSync()
        {
            Console.WriteLine("\n--- 测试复合场景 ---");
            
            // 创建TestPlayer A并进行复合修改
            var playerA = new TestPlayer();
            
            // 修改基本类型
            playerA.Name = "ComplexPlayer";
            playerA.Level = 25;
            
            // 修改普通集合（整体替换）
            playerA.NameList = new List<string> { "a", "b", "c" };
            
            // 修改同步集合
            playerA.SyncNameList = new SyncList<string>();
            playerA.SyncNameList.Add("sync_a");
            playerA.SyncNameList.Add("sync_b");
            
            playerA.SyncStats = new SyncDictionary<string, int>();
            playerA.SyncStats["health"] = 200;
            playerA.SyncStats["mana"] = 150;
            
            Console.WriteLine($"PlayerA 修改完成，脏字段数: {playerA.DirtyCount}");
            
            // 验证初始状态
            Assert.AreEqual("ComplexPlayer", playerA.Name, "Name设置应成功");
            Assert.AreEqual(25, playerA.Level, "Level设置应成功");
            Assert.AreEqual(3, playerA.NameList?.Count, "NameList应有3个元素");
            Assert.AreEqual(2, playerA.SyncNameList?.Count, "SyncNameList应有2个元素");
            Assert.AreEqual(2, playerA.SyncStats?.Count, "SyncStats应有2个元素");
            Assert.IsTrue(playerA.IsDirty, "PlayerA应处于脏状态");
            
            // 序列化
            var data = SyncHelper.CreateSyncMessage(playerA);
            Console.WriteLine($"序列化数据长度: {data.Length} bytes");
            Assert.IsTrue(data.Length > 0, "序列化数据不应为空");
            
            // 应用到TestPlayer B
            var playerB = new TestPlayer();
            SyncHelper.DeserializeObject(playerB, data);
            
            // 全面验证
            Assert.AreEqual("ComplexPlayer", playerB.Name, "Name同步应成功");
            Assert.AreEqual(25, playerB.Level, "Level同步应成功");
            Assert.AreEqual(3, playerB.NameList?.Count, "NameList同步应成功");
            Assert.AreEqual(2, playerB.SyncNameList?.Count, "SyncNameList同步应成功");
            Assert.AreEqual(2, playerB.SyncStats?.Count, "SyncStats同步应成功");
            Assert.IsTrue(playerB.SyncStats?.ContainsKey("health") == true, "应包含health键");
            Assert.AreEqual(200, playerB.SyncStats?["health"], "health值应为200");
            Assert.IsTrue(playerB.SyncStats?.ContainsKey("mana") == true, "应包含mana键");
            Assert.AreEqual(150, playerB.SyncStats?["mana"], "mana值应为150");
            
            Console.WriteLine("✓ 复合场景同步成功");
            
            // 验证PlayerB可以继续进行同步操作
            Console.WriteLine("\n测试PlayerB的继续同步操作...");
            var oldDirtyCount = playerB.DirtyCount;
            playerB.SyncNameList?.Add("test_continue");
            
            Assert.IsTrue(playerB.IsDirty, "PlayerB修改后应处于脏状态");
            Console.WriteLine($"修改前DirtyCount: {oldDirtyCount}, 修改后: {playerB.DirtyCount}");
            Console.WriteLine("✓ PlayerB可以继续同步操作");
        }

        /// <summary>
        /// 测试多轮同步：连续进行多次同步操作
        /// </summary>
        [TestMethod]
        public void TestMultipleRoundsSync()
        {
            Console.WriteLine("\n--- 测试多轮同步 ---");
            
            var playerA = new TestPlayer();
            var playerB = new TestPlayer();
            
            // 第一轮：基本属性同步
            Console.WriteLine("第一轮：基本属性同步");
            playerA.Name = "Round1Player";
            playerA.Level = 1;
            
            var data1 = SyncHelper.CreateSyncMessage(playerA);
            SyncHelper.DeserializeObject(playerB, data1);
            
            Assert.AreEqual("Round1Player", playerB.Name, "第一轮Name同步应成功");
            Assert.AreEqual(1, playerB.Level, "第一轮Level同步应成功");
            Console.WriteLine("✓ 第一轮同步成功");
            
            // 第二轮：添加集合数据
            Console.WriteLine("第二轮：添加集合数据");
            playerA.NameList = new List<string> { "item1", "item2" };
            playerA.SyncNameList = new SyncList<string>();
            playerA.SyncNameList.Add("sync1");
            
            var data2 = SyncHelper.CreateSyncMessage(playerA);
            SyncHelper.DeserializeObject(playerB, data2);
            
            Assert.AreEqual(2, playerB.NameList?.Count, "第二轮NameList同步应成功");
            Assert.AreEqual(1, playerB.SyncNameList?.Count, "第二轮SyncNameList同步应成功");
            Console.WriteLine("✓ 第二轮同步成功");
            
            // 第三轮：增量修改同步集合
            Console.WriteLine("第三轮：增量修改同步集合");
            playerA.SyncNameList.Add("sync2");
            playerA.SyncStats = new SyncDictionary<string, int>();
            playerA.SyncStats["score"] = 100;
            
            var data3 = SyncHelper.CreateSyncMessage(playerA);
            SyncHelper.DeserializeObject(playerB, data3);
            
            Assert.AreEqual(2, playerB.SyncNameList?.Count, "第三轮SyncNameList应有2个元素");
            Assert.AreEqual(1, playerB.SyncStats?.Count, "第三轮SyncStats应有1个元素");
            Assert.AreEqual(100, playerB.SyncStats?["score"], "score值应为100");
            Console.WriteLine("✓ 第三轮同步成功");
            
            Console.WriteLine("✓ 多轮同步测试完成");
        }

        /// <summary>
        /// 测试双向同步：两个TestPlayer对象相互同步
        /// </summary>
        [TestMethod]
        public void TestBidirectionalSync()
        {
            Console.WriteLine("\n--- 测试双向同步 ---");
            
            var playerA = new TestPlayer();
            var playerB = new TestPlayer();
            
            // A修改并同步到B
            Console.WriteLine("A -> B 同步");
            playerA.Name = "PlayerA";
            playerA.Level = 10;
            playerA.SyncNameList = new SyncList<string>();
            playerA.SyncNameList.Add("from_A");
            
            var dataAtoB = SyncHelper.CreateSyncMessage(playerA);
            SyncHelper.DeserializeObject(playerB, dataAtoB);
            
            Assert.AreEqual("PlayerA", playerB.Name, "A->B Name同步应成功");
            Assert.AreEqual(10, playerB.Level, "A->B Level同步应成功");
            Assert.AreEqual(1, playerB.SyncNameList?.Count, "A->B SyncNameList同步应成功");
            Console.WriteLine("✓ A -> B 同步成功");
            
            // B修改并同步到A
            Console.WriteLine("B -> A 同步");
            playerB.Name = "PlayerB";
            playerB.Level = 20;
            playerB.SyncStats = new SyncDictionary<string, int>();
            playerB.SyncStats["level"] = 5;
            
            var dataBtoA = SyncHelper.CreateSyncMessage(playerB);
            SyncHelper.DeserializeObject(playerA, dataBtoA);
            
            Assert.AreEqual("PlayerB", playerA.Name, "B->A Name同步应成功");
            Assert.AreEqual(20, playerA.Level, "B->A Level同步应成功");
            Assert.AreEqual(1, playerA.SyncStats?.Count, "B->A SyncStats同步应成功");
            Assert.AreEqual(5, playerA.SyncStats?["level"], "level值应为5");
            Console.WriteLine("✓ B -> A 同步成功");
            
            // 验证双方都有完整数据
            Console.WriteLine("验证最终状态");
            Assert.AreEqual("PlayerB", playerA.Name, "PlayerA应有最新的Name");
            Assert.AreEqual("PlayerB", playerB.Name, "PlayerB应有最新的Name");
            Assert.AreEqual(1, playerA.SyncNameList?.Count, "PlayerA应有SyncNameList");
            Assert.AreEqual(1, playerB.SyncNameList?.Count, "PlayerB应有SyncNameList");
            Assert.AreEqual(1, playerA.SyncStats?.Count, "PlayerA应有SyncStats");
            Assert.AreEqual(1, playerB.SyncStats?.Count, "PlayerB应有SyncStats");
            
            Console.WriteLine("✓ 双向同步测试完成");
        }

        /// <summary>
        /// 测试空值和边界情况
        /// </summary>
        [TestMethod]
        public void TestEdgeCases()
        {
            Console.WriteLine("\n--- 测试边界情况 ---");
            
            var playerA = new TestPlayer();
            var playerB = new TestPlayer();
            
            // 测试空字符串
            Console.WriteLine("测试空字符串");
            playerA.Name = "";
            playerA.Level = 0;
            var data1 = SyncHelper.CreateSyncMessage(playerA);
            SyncHelper.DeserializeObject(playerB, data1);
            Assert.AreEqual("", playerB.Name, "空字符串同步应成功");
            Assert.AreEqual(0, playerB.Level, "零值同步应成功");
            
            // 测试空集合
            Console.WriteLine("测试空集合");
            playerA.NameList = new List<string>();
            playerA.NameDict = new Dictionary<string, string>();
            var data2 = SyncHelper.CreateSyncMessage(playerA);
            SyncHelper.DeserializeObject(playerB, data2);
            Assert.AreEqual(0, playerB.NameList?.Count, "空List同步应成功");
            Assert.AreEqual(0, playerB.NameDict?.Count, "空Dictionary同步应成功");
            
            // 测试空的同步集合
            Console.WriteLine("测试空的同步集合");
            playerA.SyncNameList = new SyncList<string>();
            playerA.SyncStats = new SyncDictionary<string, int>();
            var data3 = SyncHelper.CreateSyncMessage(playerA);
            SyncHelper.DeserializeObject(playerB, data3);
            Assert.AreEqual(0, playerB.SyncNameList?.Count, "空SyncList同步应成功");
            Assert.AreEqual(0, playerB.SyncStats?.Count, "空SyncDictionary同步应成功");
            
            Console.WriteLine("✓ 边界情况测试完成");
        }

        /// <summary>
        /// 测试使用不同实体类型的同步
        /// </summary>
        [TestMethod]
        public void TestDifferentEntityTypes()
        {
            Console.WriteLine("\n--- 测试不同实体类型同步 ---");
            
            // 测试SimpleEntity
            var simpleA = new SimpleEntity("test123", "data456");
            var simpleB = new SimpleEntity();
            
            var data = SyncHelper.CreateSyncMessage(simpleA);
            SyncHelper.DeserializeObject(simpleB, data);
            
            Assert.AreEqual("test123", simpleB.Id, "SimpleEntity Id同步应成功");
            Assert.AreEqual("data456", simpleB.Data, "SimpleEntity Data同步应成功");
            
            // 测试CollectionEntity
            var collectionA = new CollectionEntity();
            collectionA.Items.Add("item1");
            collectionA.Scores["score1"] = 100;
            collectionA.SyncItems.Add("syncItem1");
            collectionA.SyncScores["syncScore1"] = 200;
            
            var collectionB = new CollectionEntity();
            
            var data2 = SyncHelper.CreateSyncMessage(collectionA);
            SyncHelper.DeserializeObject(collectionB, data2);
            
            Assert.AreEqual(1, collectionB.Items?.Count, "CollectionEntity Items同步应成功");
            Assert.AreEqual(1, collectionB.Scores?.Count, "CollectionEntity Scores同步应成功");
            Assert.AreEqual(1, collectionB.SyncItems?.Count, "CollectionEntity SyncItems同步应成功");
            Assert.AreEqual(1, collectionB.SyncScores?.Count, "CollectionEntity SyncScores同步应成功");
            
            Console.WriteLine("✓ 不同实体类型同步测试完成");
        }
    }
} 
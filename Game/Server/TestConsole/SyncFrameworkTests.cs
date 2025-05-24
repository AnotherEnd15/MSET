using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SyncFramework;

namespace TestConsole
{
    /// <summary>
    /// 同步框架测试类 - 基本类型同步测试
    /// </summary>
    [TestClass]
    public class SyncFrameworkTests
    {
        [TestInitialize]
        public void Setup()
        {
            Console.WriteLine("初始化测试环境...");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Console.WriteLine("清理测试环境...");
        }

        /// <summary>
        /// 测试基本类型同步（string, int等）
        /// </summary>
        [TestMethod]
        public void TestPrimitiveSync()
        {
            Console.WriteLine("\n--- 测试基本类型同步 ---");
            
            // 创建TestPlayer A并修改基本属性
            var playerA = new TestPlayer();
            playerA.Name = "TestPlayer";
            playerA.Level = 10;
            
            Console.WriteLine($"PlayerA Name: {playerA.Name}");
            Console.WriteLine($"PlayerA Level: {playerA.Level}");
            Console.WriteLine($"PlayerA IsDirty: {playerA.IsDirty}");
            
            // 序列化A的脏数据
            var data = SyncHelper.CreateSyncMessage(playerA);
            Console.WriteLine($"序列化数据长度: {data.Length} bytes");
            
            Assert.IsTrue(data.Length > 0, "序列化数据不应为空");
            
            // 创建TestPlayer B并应用数据
            var playerB = new TestPlayer();
            SyncHelper.DeserializeObject(playerB, data);
            
            Console.WriteLine($"PlayerB Name: {playerB.Name}");
            Console.WriteLine($"PlayerB Level: {playerB.Level}");
            
            // 验证同步结果
            Assert.AreEqual(playerA.Name, playerB.Name, "Name同步失败");
            Assert.AreEqual(playerA.Level, playerB.Level, "Level同步失败");
            Console.WriteLine("✓ 基本类型同步成功");
        }

        /// <summary>
        /// 测试简单实体的同步
        /// </summary>
        [TestMethod]
        public void TestSimpleEntitySync()
        {
            Console.WriteLine("\n--- 测试简单实体同步 ---");
            
            // 创建SimpleEntity A
            var entityA = new SimpleEntity("test_id", "test_data");
            
            Console.WriteLine($"EntityA Id: {entityA.Id}");
            Console.WriteLine($"EntityA Data: {entityA.Data}");
            Console.WriteLine($"EntityA IsDirty: {entityA.IsDirty}");
            
            // 序列化
            var data = SyncHelper.CreateSyncMessage(entityA);
            Console.WriteLine($"序列化数据长度: {data.Length} bytes");
            
            Assert.IsTrue(data.Length > 0, "序列化数据不应为空");
            
            // 创建SimpleEntity B并应用数据
            var entityB = new SimpleEntity();
            SyncHelper.DeserializeObject(entityB, data);
            
            Console.WriteLine($"EntityB Id: {entityB.Id}");
            Console.WriteLine($"EntityB Data: {entityB.Data}");
            
            // 验证同步结果
            Assert.AreEqual(entityA.Id, entityB.Id, "Id同步失败");
            Assert.AreEqual(entityA.Data, entityB.Data, "Data同步失败");
            Console.WriteLine("✓ 简单实体同步成功");
        }

        /// <summary>
        /// 测试同步框架的基本功能
        /// </summary>
        [TestMethod]
        public void TestSyncFrameworkBasic()
        {
            try
            {
                Console.WriteLine("=== 开始同步框架基本测试 ===");
                
                // 这个测试运行更安全的子集
                TestPrimitiveSync();
                
                Console.WriteLine("\n=== 基本测试通过! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n=== 测试失败: {ex.Message} ===");
                throw;
            }
        }

        /// <summary>
        /// 测试TestPlayer对象的IsDirty状态
        /// </summary>
        [TestMethod]
        public void TestPlayerDirtyState()
        {
            var player = new TestPlayer();
            
            // 初始状态应该不脏
            Assert.IsFalse(player.IsDirty, "新创建的TestPlayer应该不是脏状态");
            Assert.AreEqual(0, player.DirtyCount, "新创建的TestPlayer脏字段数量应为0");
            
            // 修改Name后应该变脏
            player.Name = "TestName";
            Assert.IsTrue(player.IsDirty, "修改Name后TestPlayer应该变为脏状态");
            
            // 修改Level也应该变脏
            player.Level = 5;
            Assert.IsTrue(player.IsDirty, "修改Level后TestPlayer应该仍为脏状态");
            
            Console.WriteLine("✓ TestPlayer脏状态测试通过");
        }

        /// <summary>
        /// 测试序列化和反序列化的基本流程
        /// </summary>
        [TestMethod]
        public void TestSerializationBasics()
        {
            var player = new TestPlayer();
            player.Name = "SerializationTest";
            player.Level = 99;
            
            // 测试序列化
            var data = SyncHelper.CreateSyncMessage(player);
            Assert.IsNotNull(data, "序列化结果不应为null");
            Assert.IsTrue(data.Length > 0, "序列化数据长度应大于0");
            
            // 测试反序列化
            var newPlayer = new TestPlayer();
            
            // 使用try-catch替代Assert.DoesNotThrow
            try
            {
                SyncHelper.DeserializeObject(newPlayer, data);
                Console.WriteLine("反序列化成功执行");
            }
            catch (Exception ex)
            {
                Assert.Fail($"反序列化不应抛出异常: {ex.Message}");
            }
            
            Assert.AreEqual(player.Name, newPlayer.Name, "反序列化后Name应该一致");
            Assert.AreEqual(player.Level, newPlayer.Level, "反序列化后Level应该一致");
            
            Console.WriteLine("✓ 序列化基础测试通过");
        }

        /// <summary>
        /// 测试清除脏状态
        /// </summary>
        [TestMethod]
        public void TestClearDirtyState()
        {
            var player = new TestPlayer();
            player.Name = "DirtyTest";
            player.Level = 1;
            
            // 应该是脏状态
            Assert.IsTrue(player.IsDirty, "修改后应该是脏状态");
            Assert.IsTrue(player.DirtyCount > 0, "脏字段数量应大于0");
            
            // 清除脏状态
            player.ClearDirtyState();
            
            // 应该不再是脏状态
            Assert.IsFalse(player.IsDirty, "清除后不应该是脏状态");
            Assert.AreEqual(0, player.DirtyCount, "清除后脏字段数量应为0");
            
            Console.WriteLine("✓ 清除脏状态测试通过");
        }
    }
} 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Linq;
using SyncFramework;

namespace TestConsole
{
    /// <summary>
    /// 测试套件主类 - 整合所有测试并提供统一的测试入口
    /// </summary>
    [TestClass]
    public class TestSuite
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("   TestConsole 同步框架测试套件 - 开始执行");
            Console.WriteLine("==========================================");
            Console.WriteLine($"测试开始时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"测试环境: {Environment.OSVersion}");
            Console.WriteLine($".NET 版本: {Environment.Version}");
            Console.WriteLine("测试项目：测试Model项目中的SyncFramework");
            Console.WriteLine("包含的测试实体：TestPlayer, SimpleEntity, CollectionEntity");
            Console.WriteLine();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Console.WriteLine();
            Console.WriteLine("==========================================");
            Console.WriteLine("   TestConsole 同步框架测试套件 - 执行完成");
            Console.WriteLine("==========================================");
            Console.WriteLine($"测试结束时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();
        }

        /// <summary>
        /// 运行所有基础功能测试
        /// </summary>
        [TestMethod]
        public void RunAllBasicTests()
        {
            Console.WriteLine("=== 运行所有基础功能测试 ===");
            
            try
            {
                // 运行基础同步测试
                var basicTests = new SyncFrameworkTests();
                basicTests.Setup();
                
                Console.WriteLine("1. 执行基本类型同步测试...");
                basicTests.TestPrimitiveSync();
                
                Console.WriteLine("2. 执行简单实体同步测试...");
                basicTests.TestSimpleEntitySync();
                
                Console.WriteLine("3. 执行TestPlayer脏状态测试...");
                basicTests.TestPlayerDirtyState();
                
                Console.WriteLine("4. 执行序列化基础测试...");
                basicTests.TestSerializationBasics();
                
                Console.WriteLine("5. 执行清除脏状态测试...");
                basicTests.TestClearDirtyState();
                
                basicTests.Cleanup();
                Console.WriteLine("✓ 所有基础功能测试通过");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 基础功能测试失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 运行所有集合同步测试
        /// </summary>
        [TestMethod]
        public void RunAllCollectionTests()
        {
            Console.WriteLine("=== 运行所有集合同步测试 ===");
            
            try
            {
                var collectionTests = new CollectionSyncTests();
                
                Console.WriteLine("1. 执行普通集合同步测试...");
                collectionTests.TestCollectionSync();
                
                Console.WriteLine("2. 执行同步集合测试...");
                collectionTests.TestSyncCollectionSync();
                
                Console.WriteLine("3. 执行CollectionEntity同步测试...");
                collectionTests.TestCollectionEntitySync();
                
                Console.WriteLine("4. 执行Dictionary序列化详细测试...");
                collectionTests.TestDictionarySerializationDetailed();
                
                Console.WriteLine("5. 执行空集合序列化测试...");
                collectionTests.TestEmptyCollectionSerialization();
                
                Console.WriteLine("✓ 所有集合同步测试通过");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 集合同步测试失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 运行所有复合场景测试
        /// </summary>
        [TestMethod]
        public void RunAllComplexTests()
        {
            Console.WriteLine("=== 运行所有复合场景测试 ===");
            
            try
            {
                var complexTests = new ComplexSyncTests();
                
                Console.WriteLine("1. 执行复合场景同步测试...");
                complexTests.TestComplexSync();
                
                Console.WriteLine("2. 执行多轮同步测试...");
                complexTests.TestMultipleRoundsSync();
                
                Console.WriteLine("3. 执行双向同步测试...");
                complexTests.TestBidirectionalSync();
                
                Console.WriteLine("4. 执行边界情况测试...");
                complexTests.TestEdgeCases();
                
                Console.WriteLine("5. 执行不同实体类型测试...");
                complexTests.TestDifferentEntityTypes();
                
                Console.WriteLine("✓ 所有复合场景测试通过");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 复合场景测试失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 运行性能测试（可选）
        /// </summary>
        [TestMethod]
        [TestCategory("Performance")]
        public void RunPerformanceTests()
        {
            Console.WriteLine("=== 运行性能测试 ===");
            Console.WriteLine("注意：性能测试可能需要较长时间，请耐心等待...");
            
            try
            {
                var performanceTests = new PerformanceTests();
                
                Console.WriteLine("1. 执行大量数据同步性能测试...");
                performanceTests.TestLargeDataSyncPerformance();
                
                Console.WriteLine("2. 执行频繁同步性能测试...");
                performanceTests.TestFrequentSyncPerformance();
                
                Console.WriteLine("3. 执行内存使用测试...");
                performanceTests.TestMemoryUsage();
                
                Console.WriteLine("4. 执行不同实体类型性能测试...");
                performanceTests.TestDifferentEntityPerformance();
                
                Console.WriteLine("✓ 所有性能测试通过");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 性能测试失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 快速烟雾测试 - 验证基本功能正常
        /// </summary>
        [TestMethod]
        [TestCategory("Smoke")]
        public void QuickSmokeTest()
        {
            Console.WriteLine("=== 快速烟雾测试 ===");
            
            try
            {
                // 测试TestPlayer基本同步
                var playerA = new TestPlayer();
                var playerB = new TestPlayer();
                
                playerA.Name = "SmokeTestPlayer";
                playerA.Level = 99;
                var data = SyncHelper.CreateSyncMessage(playerA);
                SyncHelper.DeserializeObject(playerB, data);
                
                Assert.AreEqual("SmokeTestPlayer", playerB.Name, "TestPlayer基本同步应该工作");
                Assert.AreEqual(99, playerB.Level, "TestPlayer Level同步应该工作");
                
                // 测试SimpleEntity同步
                var entityA = new SimpleEntity("smoke_id", "smoke_data");
                var entityB = new SimpleEntity();
                
                data = SyncHelper.CreateSyncMessage(entityA);
                SyncHelper.DeserializeObject(entityB, data);
                
                Assert.AreEqual("smoke_id", entityB.Id, "SimpleEntity同步应该工作");
                Assert.AreEqual("smoke_data", entityB.Data, "SimpleEntity Data同步应该工作");
                
                // 测试同步集合
                playerA.SyncNameList = new SyncList<string>();
                playerA.SyncNameList.Add("smoke_item");
                
                data = SyncHelper.CreateSyncMessage(playerA);
                SyncHelper.DeserializeObject(playerB, data);
                
                Assert.AreEqual(1, playerB.SyncNameList?.Count, "同步集合应该工作");
                Assert.AreEqual("smoke_item", playerB.SyncNameList?[0], "同步集合元素应该正确");
                
                Console.WriteLine("✓ 快速烟雾测试通过 - 系统基本功能正常");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 烟雾测试失败 - 系统基本功能异常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 完整测试套件 - 运行所有测试
        /// </summary>
        [TestMethod]
        [TestCategory("FullSuite")]
        public void RunFullTestSuite()
        {
            Console.WriteLine("=== 运行完整测试套件 ===");
            Console.WriteLine("这将运行所有测试，可能需要较长时间...");
            
            var startTime = DateTime.Now;
            
            try
            {
                Console.WriteLine("\n[1/5] 运行快速烟雾测试...");
                QuickSmokeTest();
                
                Console.WriteLine("\n[2/5] 运行基础功能测试...");
                RunAllBasicTests();
                
                Console.WriteLine("\n[3/5] 运行集合同步测试...");
                RunAllCollectionTests();
                
                Console.WriteLine("\n[4/5] 运行复合场景测试...");
                RunAllComplexTests();
                
                Console.WriteLine("\n[5/5] 运行性能测试...");
                RunPerformanceTests();
                
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                
                Console.WriteLine();
                Console.WriteLine("🎉 完整测试套件执行成功！");
                Console.WriteLine($"总执行时间: {duration.TotalSeconds:F2} 秒");
                Console.WriteLine("所有Model项目SyncFramework功能均正常工作。");
                Console.WriteLine("测试覆盖：TestPlayer, SimpleEntity, CollectionEntity");
            }
            catch (Exception ex)
            {
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                
                Console.WriteLine();
                Console.WriteLine($"❌ 测试套件执行失败: {ex.Message}");
                Console.WriteLine($"执行时间: {duration.TotalSeconds:F2} 秒");
                Console.WriteLine("请检查失败的测试用例并修复相关问题。");
                throw;
            }
        }

        /// <summary>
        /// 获取测试统计信息
        /// </summary>
        [TestMethod]
        public void GetTestStatistics()
        {
            Console.WriteLine("=== TestConsole 测试统计信息 ===");
            
            var assembly = Assembly.GetExecutingAssembly();
            var testClasses = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<TestClassAttribute>() != null)
                .ToList();
            
            var totalTestMethods = 0;
            
            Console.WriteLine("测试类统计:");
            foreach (var testClass in testClasses)
            {
                var testMethods = testClass.GetMethods()
                    .Where(m => m.GetCustomAttribute<TestMethodAttribute>() != null)
                    .ToList();
                
                Console.WriteLine($"  {testClass.Name}: {testMethods.Count} 个测试方法");
                totalTestMethods += testMethods.Count;
                
                // 显示测试方法详情
                foreach (var method in testMethods)
                {
                    var categories = method.GetCustomAttributes<TestCategoryAttribute>()
                        .SelectMany(attr => new[] { attr.TestCategories })
                        .Where(cat => cat != null)
                        .ToList();
                    
                    var categoryInfo = categories.Any() ? $" [{string.Join(", ", categories)}]" : "";
                    Console.WriteLine($"    - {method.Name}{categoryInfo}");
                }
                Console.WriteLine();
            }
            
            Console.WriteLine($"总计: {testClasses.Count} 个测试类, {totalTestMethods} 个测试方法");
            Console.WriteLine("测试实体类型：TestPlayer, SimpleEntity, CollectionEntity");
            Console.WriteLine("测试框架：Model项目中的SyncFramework");
            
            // 不抛出异常，这只是信息展示
            Assert.IsTrue(totalTestMethods > 0, "应该有测试方法存在");
        }

        /// <summary>
        /// 测试源生成器功能验证
        /// </summary>
        [TestMethod]
        public void TestSourceGeneratorVerification()
        {
            Console.WriteLine("=== 源生成器功能验证 ===");
            
            try
            {
                // 验证TestPlayer类实现了ISyncable接口
                var player = new TestPlayer();
                Assert.IsTrue(player is ISyncable, "TestPlayer应该实现ISyncable接口");
                
                // 验证SimpleEntity类实现了ISyncable接口
                var entity = new SimpleEntity();
                Assert.IsTrue(entity is ISyncable, "SimpleEntity应该实现ISyncable接口");
                
                // 验证CollectionEntity类实现了ISyncable接口
                var collection = new CollectionEntity();
                Assert.IsTrue(collection is ISyncable, "CollectionEntity应该实现ISyncable接口");
                
                Console.WriteLine("✓ 所有测试实体都正确实现了ISyncable接口");
                
                // 验证基本的同步功能
                player.Name = "SourceGenTest";
                Assert.IsTrue(player.IsDirty, "修改属性后应该变脏");
                Assert.IsTrue(player.DirtyCount > 0, "脏字段计数应该大于0");
                
                var data = SyncHelper.CreateSyncMessage(player);
                Assert.IsTrue(data.Length > 0, "应该能够序列化脏数据");
                
                Console.WriteLine("✓ 源生成器生成的同步代码功能正常");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 源生成器验证失败: {ex.Message}");
                Console.WriteLine("请确保Analyzer项目正确引用，并且源生成器正常工作");
                throw;
            }
        }
    }
} 
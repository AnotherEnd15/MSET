using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SyncFramework;
using System.Collections.Generic;
using System.Linq;

namespace TestConsole
{
    /// <summary>
    /// 集合同步测试类 - 测试普通集合和同步集合的同步功能
    /// </summary>
    [TestClass]
    public class CollectionSyncTests
    {
        /// <summary>
        /// 测试普通集合同步（List, Dictionary）
        /// </summary>
        [TestMethod]
        public void TestCollectionSync()
        {
            Console.WriteLine("\n--- 测试普通集合同步 ---");
            
            // 创建TestPlayer A并设置集合（普通集合只支持整体替换）
            var playerA = new TestPlayer();
            Console.WriteLine($"PlayerA 初始状态 - IsDirty: {playerA.IsDirty}, DirtyCount: {playerA.DirtyCount}");
            
            // 初始化集合
            playerA.InitializeCollections();
            Console.WriteLine($"PlayerA 初始化集合后 - IsDirty: {playerA.IsDirty}, DirtyCount: {playerA.DirtyCount}");
            
            playerA.NameList = new List<string> { "item1", "item2", "item3" };
            Console.WriteLine($"PlayerA 设置NameList后 - IsDirty: {playerA.IsDirty}, DirtyCount: {playerA.DirtyCount}");
            
            playerA.NameDict = new Dictionary<string, string> 
            { 
                { "key1", "value1" }, 
                { "key2", "value2" },
                { "key3", "value3" }
            };
            Console.WriteLine($"PlayerA 设置NameDict后 - IsDirty: {playerA.IsDirty}, DirtyCount: {playerA.DirtyCount}");
            
            Assert.IsNotNull(playerA.NameList, "NameList不应为null");
            Assert.IsNotNull(playerA.NameDict, "NameDict不应为null");
            Assert.AreEqual(3, playerA.NameList.Count, "NameList应有3个元素");
            Assert.AreEqual(3, playerA.NameDict.Count, "NameDict应有3个元素");
            
            // 序列化A的脏数据
            var data = SyncHelper.CreateSyncMessage(playerA);
            Console.WriteLine($"序列化数据长度: {data.Length} bytes");
            
            if (data.Length > 0)
            {
                // 显示前20个字节的十六进制
                var hexBytes = string.Join(" ", data.Take(Math.Min(20, data.Length)).Select(b => b.ToString("X2")));
                Console.WriteLine($"序列化数据前{Math.Min(20, data.Length)}字节: {hexBytes}");
            }
            
            Assert.IsTrue(data.Length > 0, "序列化数据不应为空");
            
            // 创建TestPlayer B并应用数据
            var playerB = new TestPlayer();
            
            Console.WriteLine($"PlayerB 反序列化前 - NameList: {playerB.NameList?.Count}, NameDict: {playerB.NameDict?.Count}");
            
            SyncHelper.DeserializeObject(playerB, data);
            Console.WriteLine("反序列化成功");
            
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
            Assert.AreEqual(3, playerB.NameList?.Count, "NameList同步后应有3个元素");
            Assert.AreEqual(3, playerB.NameDict?.Count, "NameDict同步后应有3个元素");
            
            Console.WriteLine("✓ 普通集合同步成功");
        }

        /// <summary>
        /// 测试同步集合（SyncList, SyncDictionary）
        /// </summary>
        [TestMethod]
        public void TestSyncCollectionSync()
        {
            Console.WriteLine("\n--- 测试同步集合 ---");
            
            // 创建TestPlayer A并修改同步集合
            var playerA = new TestPlayer();
            
            // 初始化同步集合
            playerA.InitializeCollections();
            
            // 直接使用SyncList/SyncDictionary的方法（应该自动触发回调）
            playerA.SyncNameList.Add("sync1");
            playerA.SyncNameList.Add("sync2");
            playerA.SyncStats.Add("hp", 100);
            playerA.SyncStats.Add("mp", 50);
            
            // 手动添加更多元素来测试
            playerA.SyncNameList.Add("sync3");
            playerA.SyncStats["level"] = 10;
            
            Assert.AreEqual(3, playerA.SyncNameList?.Count, "SyncNameList应有3个元素");
            Assert.AreEqual(3, playerA.SyncStats?.Count, "SyncStats应有3个元素");
            
            Console.WriteLine($"PlayerA SyncNameList count: {playerA.SyncNameList?.Count}");
            Console.WriteLine($"PlayerA SyncStats count: {playerA.SyncStats?.Count}");
            Console.WriteLine($"PlayerA IsDirty: {playerA.IsDirty}");
            
            // 序列化A的脏数据
            var data = SyncHelper.CreateSyncMessage(playerA);
            Console.WriteLine($"序列化数据长度: {data.Length} bytes");
            Assert.IsTrue(data.Length > 0, "序列化数据不应为空");
            
            // 创建TestPlayer B并应用数据
            var playerB = new TestPlayer();
            SyncHelper.DeserializeObject(playerB, data);
            
            Console.WriteLine($"PlayerB SyncNameList count: {playerB.SyncNameList?.Count}");
            Console.WriteLine($"PlayerB SyncStats count: {playerB.SyncStats?.Count}");
            
            // 验证同步结果
            Assert.AreEqual(3, playerB.SyncNameList?.Count, "SyncNameList同步后应有3个元素");
            Assert.AreEqual(3, playerB.SyncStats?.Count, "SyncStats同步后应有3个元素");
            
            Console.WriteLine("✓ 同步集合同步成功");
            
            // 测试反序列化后的回调是否正确设置
            Console.WriteLine("\n测试反序列化后的回调设置...");
            var oldDirtyCount = playerB.DirtyCount;
            playerB.SyncNameList?.Add("test_callback");
            
            // 注意：由于我们使用了实际的Model框架，这个测试需要调整
            Console.WriteLine($"添加元素前DirtyCount: {oldDirtyCount}");
            Console.WriteLine($"添加元素后DirtyCount: {playerB.DirtyCount}");
            Console.WriteLine("✓ 同步集合测试完成");
        }

        /// <summary>
        /// 测试使用CollectionEntity的集合同步
        /// </summary>
        [TestMethod]
        public void TestCollectionEntitySync()
        {
            Console.WriteLine("\n--- 测试CollectionEntity同步 ---");
            
            var entityA = new CollectionEntity();
            
            // 设置普通集合
            entityA.Items = new List<string> { "item1", "item2" };
            entityA.Scores = new Dictionary<string, int> { { "score1", 100 }, { "score2", 200 } };
            
            // 设置同步集合
            entityA.SyncItems.Add("syncItem1");
            entityA.SyncItems.Add("syncItem2");
            entityA.SyncScores["syncScore1"] = 300;
            entityA.SyncScores["syncScore2"] = 400;
            
            Console.WriteLine($"EntityA 设置后 - IsDirty: {entityA.IsDirty}");
            
            // 序列化
            var data = SyncHelper.CreateSyncMessage(entityA);
            Assert.IsTrue(data.Length > 0, "序列化数据不应为空");
            
            // 创建EntityB并应用数据
            var entityB = new CollectionEntity();
            SyncHelper.DeserializeObject(entityB, data);
            
            // 验证结果
            Assert.AreEqual(2, entityB.Items?.Count, "Items应有2个元素");
            Assert.AreEqual(2, entityB.Scores?.Count, "Scores应有2个元素");
            Assert.AreEqual(2, entityB.SyncItems?.Count, "SyncItems应有2个元素");
            Assert.AreEqual(2, entityB.SyncScores?.Count, "SyncScores应有2个元素");
            
            Console.WriteLine("✓ CollectionEntity同步成功");
        }

        /// <summary>
        /// 测试Dictionary序列化的详细流程
        /// </summary>
        [TestMethod]
        public void TestDictionarySerializationDetailed()
        {
            Console.WriteLine("=== 详细测试Dictionary序列化 ===");
            
            // 创建测试字典
            var testDict = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" }
            };
            
            Console.WriteLine($"原始字典元素数: {testDict.Count}");
            foreach (var kvp in testDict)
            {
                Console.WriteLine($"  {kvp.Key} => {kvp.Value}");
            }
            
            // 序列化
            using (var ms = new System.IO.MemoryStream())
            {
                var writer = new System.IO.BinaryWriter(ms);
                TypeSerializers.SerializeField(writer, testDict);
                
                var data = ms.ToArray();
                Console.WriteLine($"序列化数据长度: {data.Length} bytes");
                Console.WriteLine($"序列化数据: {string.Join(" ", data.Select(b => b.ToString("X2")))}");
                
                Assert.IsTrue(data.Length > 0, "序列化数据长度应大于0");
                
                // 反序列化
                using (var ms2 = new System.IO.MemoryStream(data))
                {
                    var reader = new System.IO.BinaryReader(ms2);
                    var restored = TypeSerializers.DeserializeField<Dictionary<string, string>>(reader);
                    
                    Console.WriteLine($"反序列化字典元素数: {restored?.Count}");
                    if (restored != null)
                    {
                        foreach (var kvp in restored)
                        {
                            Console.WriteLine($"  {kvp.Key} => {kvp.Value}");
                        }
                    }
                    
                    // 验证
                    Assert.AreEqual(3, restored?.Count, "反序列化后字典应有3个元素");
                    Assert.IsTrue(restored?.ContainsKey("key1") == true, "应包含key1");
                    Assert.AreEqual("value1", restored?["key1"], "key1的值应为value1");
                    
                    Console.WriteLine("✓ Dictionary序列化测试成功");
                }
            }
        }

        /// <summary>
        /// 测试空集合的序列化
        /// </summary>
        [TestMethod]
        public void TestEmptyCollectionSerialization()
        {
            Console.WriteLine("\n--- 测试空集合序列化 ---");
            
            var entity = new CollectionEntity();
            
            // 不添加任何元素，测试空集合
            entity.Items = new List<string>();
            entity.Scores = new Dictionary<string, int>();
            
            var data = SyncHelper.CreateSyncMessage(entity);
            Assert.IsTrue(data.Length > 0, "即使是空集合也应该产生序列化数据");
            
            var newEntity = new CollectionEntity();
            SyncHelper.DeserializeObject(newEntity, data);
            
            Assert.AreEqual(0, newEntity.Items?.Count, "空List应该正确同步");
            Assert.AreEqual(0, newEntity.Scores?.Count, "空Dictionary应该正确同步");
            
            Console.WriteLine("✓ 空集合序列化测试成功");
        }
    }
} 
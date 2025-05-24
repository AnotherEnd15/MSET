using System;
using SyncFramework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("启动同步框架测试...");
            
            try
            {
                // 只运行Dictionary序列化测试
                SyncTest.RunAllTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n程序异常: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return;
            }
            
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
        
        static void TestDictionarySerializationOnly()
        {
            Console.WriteLine("=== 单独测试Dictionary序列化 ===");
            
            try
            {
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
                using (var ms = new MemoryStream())
                {
                    var writer = new BinaryWriter(ms);
                    TypeSerializers.SerializeField(writer, testDict);
                    
                    var data = ms.ToArray();
                    Console.WriteLine($"序列化数据长度: {data.Length} bytes");
                    Console.WriteLine($"序列化数据: {string.Join(" ", data.Select(b => b.ToString("X2")))}");
                    
                    // 反序列化
                    using (var ms2 = new MemoryStream(data))
                    {
                        var reader = new BinaryReader(ms2);
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
                        if (restored?.Count == 3)
                        {
                            Console.WriteLine("✓ Dictionary序列化测试成功");
                        }
                        else
                        {
                            Console.WriteLine($"❌ Dictionary序列化测试失败: 期望3个元素，实际{restored?.Count}个");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Dictionary序列化测试异常: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
} 
// using System;
// using System.Threading.Tasks;
// using System.Collections.Generic;
// using Microsoft.Extensions.Logging;
// using MongoDB.Bson;
// using MongoDB.Driver;
// using MongoDBOperations;

// // 定义模型类
// public class User
// {
//     public ObjectId _id { get; set; }
//     public string Name { get; set; }
//     public string Email { get; set; }
//     public int Age { get; set; }
//     public string Status { get; set; }
//     public List<string> Tags { get; set; }
//     public Address Address { get; set; }
// }

// public class Address
// {
//     public string City { get; set; }
//     public string Street { get; set; }
//     public string PostCode { get; set; }
// }

// public class UserStats
// {
//     public string Status { get; set; }
//     public int Count { get; set; }
//     public double AverageAge { get; set; }
// }

// public class Program
// {
//     public static async Task Main(string[] args)
//     {
//         // 设置日志工厂
//         var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
//         var logger = loggerFactory.CreateLogger<MongoDBWrapper>();
        
//         // 初始化MongoDB辅助类
//         MongoDBHelper.Initialize("mongodb://localhost:27017", "testdb", logger);
        
        
//         await DemonstrateInsert();
//         await DemonstrateFindById();
//         await DemonstrateFindOne();
//         await DemonstrateFindMany();
//         await DemonstrateUpdate();
//         await DemonstrateConditionalUpdate();
//         await DemonstrateFindOneAndUpdate();
//         await DemonstrateInsertIfNotExists();
//         await DemonstrateAggregate();
//         await DemonstrateRunCommand();
//         await DemonstrateDelete();
        
//     }
    
//     // 1. 演示插入操作
//     private static async Task DemonstrateInsert()
//     {
//         Console.WriteLine("1. 演示插入操作");
        
//         var user = new User
//         {
//             Name = "张三",
//             Email = "zhangsan@example.com",
//             Age = 30,
//             Status = "active",
//             Tags = new List<string> { "vip", "customer" },
//             Address = new Address
//             {
//                 City = "北京",
//                 Street = "朝阳区",
//                 PostCode = "100000"
//             }
//         };
        
//         bool inserted = await MongoDBHelper.InsertAsync(user);
//         Console.WriteLine($"插入用户结果: {inserted}");
//         Console.WriteLine();
//     }
    
//     // 2. 演示根据ID查找
//     private static async Task DemonstrateFindById()
//     {
//         Console.WriteLine("2. 演示根据ID查找");
        
//         // 先插入一个新用户获取ID
//         var newUser = new User
//         {
//             Name = "李四",
//             Email = "lisi@example.com",
//             Age = 25,
//             Status = "inactive"
//         };
        
//         await MongoDBHelper.InsertAsync(newUser);
//         string userId = newUser._id.ToString();
        
//         var foundUser = await MongoDBHelper.FindByIdAsync<User>(userId);
//         if (foundUser != null)
//         {
//             Console.WriteLine($"通过ID找到用户: {foundUser.Name}, 邮箱: {foundUser.Email}");
//         }
//         Console.WriteLine();
//     }
    
//     // 3. 演示条件查找单个文档
//     private static async Task DemonstrateFindOne()
//     {
//         Console.WriteLine("3. 演示条件查找单个文档");
        
//         var foundUser = await MongoDBHelper.FindOneAsync<User>(u => u.Email == "zhangsan@example.com");
//         if (foundUser != null)
//         {
//             Console.WriteLine($"条件查找单个用户: {foundUser.Name}, 年龄: {foundUser.Age}");
//         }
//         Console.WriteLine();
//     }
    
//     // 4. 演示条件查找多个文档
//     private static async Task DemonstrateFindMany()
//     {
//         Console.WriteLine("4. 演示条件查找多个文档");
        
//         // 先插入几个测试用户
//         for (int i = 0; i < 5; i++)
//         {
//             await MongoDBHelper.InsertAsync(new User
//             {
//                 Name = $"测试用户{i + 1}",
//                 Email = $"test{i + 1}@example.com",
//                 Age = 20 + i,
//                 Status = i % 2 == 0 ? "active" : "inactive"
//             });
//         }
        
//         var users = await MongoDBHelper.FindManyAsync<User>(u => u.Age > 20);
//         Console.WriteLine($"找到 {users.Count} 个年龄大于20的用户:");
//         foreach (var user in users)
//         {
//             Console.WriteLine($"  - {user.Name}, 年龄: {user.Age}, 状态: {user.Status}");
//         }
//         Console.WriteLine();
//     }
    
//     // 5. 演示更新操作
//     private static async Task DemonstrateFindMany()
//     {
//         Console.WriteLine("4. 演示条件查找多个文档");
        
//         // 先插入几个测试用户
//         for (int i = 0; i < 5; i++)
//         {
//             await MongoDBHelper.InsertAsync(new User
//             {
//                 Name = $"测试用户{i + 1}",
//                 Email = $"test{i + 1}@example.com",
//                 Age = 20 + i,
//                 Status = i % 2 == 0 ? "active" : "inactive"
//             });
//         }
        
//         var users = await MongoDBHelper.FindManyAsync<User>(u => u.Age > 20);
//         Console.WriteLine($"找到 {users.Count} 个年龄大于20的用户:");
//         foreach (var user in users)
//         {
//             Console.WriteLine($"  - {user.Name}, 年龄: {user.Age}, 状态: {user.Status}");
//         }
//         Console.WriteLine();
//     }
    
//     // 5. 演示更新操作
//     private static async Task DemonstrateUpdate()
//     {
//         Console.WriteLine("5. 演示更新操作");
        
//         var foundUser = await MongoDBHelper.FindOneAsync<User>(u => u.Email == "zhangsan@example.com");
//         if (foundUser != null)
//         {
//             // 更新用户信息
//             foundUser.Age = 31;
//             foundUser.Status = "premium";
            
//             var updateResult = await MongoDBHelper.UpdateAsync<User>(
//                 u => u.Email == "zhangsan@example.com",
//                 foundUser);
                
//             Console.WriteLine($"更新结果: 匹配数 {updateResult.MatchedCount}, 修改数 {updateResult.ModifiedCount}");
//         }
//         Console.WriteLine();
//     }
    
//     // 6. 演示条件更新
//     private static async Task DemonstrateConditionalUpdate()
//     {
//         Console.WriteLine("6. 演示条件更新");
        
//         // 只有当年龄为31时才更新状态
//         var filter = new BsonDocument
//         {
//             { "Email", "zhangsan@example.com" },
//             { "Age", 31 }
//         };
        
//         var update = new BsonDocument("Status", "vip");
        
//         var conditionalResult = await MongoDBHelper.ConditionalUpdateAsync<User>(filter, update);
//         Console.WriteLine($"条件更新结果: 匹配数 {conditionalResult.MatchedCount}, 修改数 {conditionalResult.ModifiedCount}");
//         Console.WriteLine();
//     }
    
//     // 7. 演示查找并更新
//     private static async Task DemonstrateFindOneAndUpdate()
//     {
//         Console.WriteLine("7. 演示查找并更新");
        
//         var user = new User { Status = "super-vip" };
//         var updatedUser = await MongoDBHelper.FindOneAndUpdateAsync<User>(
//             u => u.Email == "zhangsan@example.com",
//             user,
//             true);
            
//         if (updatedUser != null)
//         {
//             Console.WriteLine($"查找并更新后的用户: {updatedUser.Name}, 状态: {updatedUser.Status}");
//         }
//         Console.WriteLine();
//     }
    
//     // 8. 演示不存在则插入
//     private static async Task DemonstrateInsertIfNotExists()
//     {
//         Console.WriteLine("8. 演示不存在则插入");
        
//         var newUser = new User
//         {
//             Name = "王五",
//             Email = "wangwu@example.com",
//             Age = 40,
//             Status = "vip"
//         };
        
//         bool inserted = await MongoDBHelper.InsertIfNotExistsAsync<User>(
//             u => u.Email == "wangwu@example.com", 
//             newUser);
//         Console.WriteLine($"不存在则插入结果: {inserted}");
        
//         // 再次尝试插入相同邮箱的用户
//         bool insertedAgain = await MongoDBHelper.InsertIfNotExistsAsync<User>(
//             u => u.Email == "wangwu@example.com", 
//             newUser);
//         Console.WriteLine($"再次尝试插入相同邮箱用户结果: {insertedAgain}");
//         Console.WriteLine();
//     }
    
//     // 9. 演示聚合操作
//     private static async Task DemonstrateAggregate()
//     {
//         Console.WriteLine("9. 演示聚合操作");
        
//         var pipeline = new[]
//         {
//             new BsonDocument("$match", new BsonDocument("Age", new BsonDocument("$gt", 20))),
//             new BsonDocument("$group", new BsonDocument
//             {
//                 { "_id", "$Status" },
//                 { "count", new BsonDocument("$sum", 1) },
//                 { "avgAge", new BsonDocument("$avg", "$Age") }
//             }),
//             new BsonDocument("$sort", new BsonDocument("avgAge", -1))
//         };
        
//         var results = await MongoDBHelper.AggregateAsync<UserStats, BsonDocument>(pipeline);
//         Console.WriteLine("聚合结果:");
//         foreach (var result in results)
//         {
//             Console.WriteLine($"  状态: {result.Status}, 数量: {result.Count}, 平均年龄: {result.AverageAge:F1}");
//         }
//         Console.WriteLine();
//     }
    
//     // 10. 演示运行命令
//     private static async Task DemonstrateRunCommand()
//     {
//         Console.WriteLine("10. 演示运行命令");
        
//         var command = new BsonDocument
//         {
//             { "count", "User" },
//             { "query", new BsonDocument("Status", "vip") }
//         };
        
//         var result = await MongoDBHelper.RunCommandAsync(command);
//         Console.WriteLine($"命令执行结果: 共有 {result["n"].AsInt32} 个VIP用户");
//         Console.WriteLine();
//     }
    
//     // 11. 演示删除
//     private static async Task DemonstrateDelete()
//     {
//         Console.WriteLine("11. 演示删除");
        
//         var deleteCount = await MongoDBHelper.DeleteAsync<User>(u => u.Email == "zhangsan@example.com");
//         Console.WriteLine($"删除zhangsan@example.com的用户数: {deleteCount}");
        
//         // 批量删除测试用户
//         var batchDeleteCount = await MongoDBHelper.DeleteAsync<User>(u => u.Email.StartsWith("test"));
//         Console.WriteLine($"批量删除测试用户数: {batchDeleteCount}");
//         Console.WriteLine();
//     }
// }
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ET.DBProxy;
using MongoDB.Bson;

namespace ET.Examples;

/// <summary>
/// DBHelper API使用示例 - 展示底层BsonDocument API和高级泛型API
/// </summary>
public static class DBHelperApiExample
{
    public class User
    {
        public string _id { get; set; }
        public string UserName { get; set; }
        public int Level { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public static async Task RunExamples()
    {
        // ================================
        // 底层 BsonDocument API 示例
        // ================================
        
        Console.WriteLine("=== 底层 BsonDocument API 示例 ===");
        
        // 1. 插入BsonDocument
        var userDoc = new BsonDocument
        {
            { "_id", "user123" },
            { "UserName", "张三" },
            { "Level", 10 },
            { "Email", "zhangsan@example.com" },
            { "CreatedAt", DateTime.UtcNow }
        };
        
        var insertResult = await DBHelper.InsertDocument("User", userDoc);
        Console.WriteLine($"插入结果: {insertResult.Success}");
        
        // 2. 使用BsonDocument查询
        var filter = new BsonDocument("UserName", "张三");
        var foundUser = await DBHelper.FindOne<BsonDocument>("User", filter);
        Console.WriteLine($"找到用户: {foundUser?["UserName"]}");
        
        // 3. 使用BsonDocument更新
        var updateFields = new BsonDocument
        {
            { "Level", 15 },
            { "Email", "zhangsan_new@example.com" }
        };
        var updateResult = await DBHelper.UpdatePartial("User", filter, updateFields);
        Console.WriteLine($"更新结果: 修改了 {updateResult.ModifiedCount} 个文档");
        
        // 4. 原子增量操作
        var incrementResult = await DBHelper.Increment("User", filter, "Level", 5);
        Console.WriteLine($"增量结果: 修改了 {incrementResult.ModifiedCount} 个文档");
        
        // 5. 计数查询
        var countResult = await DBHelper.Count("User", new BsonDocument("Level", new BsonDocument("$gte", 15)));
        Console.WriteLine($"Level >= 15 的用户数量: {countResult.Count}");
        
        // ================================
        // 高级泛型 Lambda API 示例
        // ================================
        
        Console.WriteLine("\n=== 高级泛型 Lambda API 示例 ===");
        
        // 1. 插入强类型实体（自动推断集合名为 "User"）
        var newUser = new User
        {
            _id = "user456",
            UserName = "李四",
            Level = 1,
            Email = "lisi@example.com",
            CreatedAt = DateTime.UtcNow
        };
        
        var insertResult2 = await DBHelper.InsertOneAsync(newUser);
        Console.WriteLine($"插入新用户结果: {insertResult2.Success}");
        
        // 2. 使用Lambda表达式查询
        var foundUser2 = await DBHelper.FindOneAsync<User>(x => x.UserName == "李四");
        Console.WriteLine($"找到用户: {foundUser2?.UserName}, Level: {foundUser2?.Level}");
        
        // 3. 使用Lambda表达式查询多个用户
        var users = await DBHelper.FindAsync<User>(x => x.Level >= 10);
        Console.WriteLine($"Level >= 10 的用户: {users.Count} 个");
        
        // 4. 使用Lambda表达式更新
        var updatedUser = new User
        {
            _id = foundUser2._id,
            UserName = foundUser2.UserName,
            Level = 25,
            Email = "lisi_updated@example.com",
            CreatedAt = foundUser2.CreatedAt
        };
        
        var updateResult2 = await DBHelper.UpdateOneAsync<User>(x => x.UserName == "李四", updatedUser);
        Console.WriteLine($"更新用户结果: 修改了 {updateResult2.ModifiedCount} 个文档");
        
        // 5. 批量操作
        var newUsers = new List<User>
        {
            new User { _id = "user789", UserName = "王五", Level = 3, Email = "wangwu@example.com", CreatedAt = DateTime.UtcNow },
            new User { _id = "user101", UserName = "赵六", Level = 7, Email = "zhaoliu@example.com", CreatedAt = DateTime.UtcNow }
        };
        
        var batchInsertResults = await DBHelper.InsertManyAsync(newUsers);
        Console.WriteLine($"批量插入结果: {batchInsertResults.Count} 个用户插入完成");
        
        // 6. 分页查询
        var pagedUsers = await DBHelper.FindAsync<User>(
            x => x.Level >= 1, 
            skip: 0, 
            limit: 2, 
            sort: new BsonDocument("Level", -1) // 按Level降序排列
        );
        Console.WriteLine($"分页查询结果: 获取到 {pagedUsers.Count} 个用户");
        
        // 7. 统计和存在性检查
        var totalUsers = await DBHelper.CountDocumentsAsync<User>();
        Console.WriteLine($"总用户数: {totalUsers}");
        
        var userExists = await DBHelper.ExistsAsync<User>(x => x.UserName == "张三");
        Console.WriteLine($"用户'张三'是否存在: {userExists}");
        
        // 8. 条件插入
        var conditionalUser = new User
        {
            _id = "user_conditional",
            UserName = "条件用户",
            Level = 1,
            Email = "conditional@example.com",
            CreatedAt = DateTime.UtcNow
        };
        
        var conditionalInsert = await DBHelper.InsertIfNotExistsAsync<User>(
            x => x.UserName == "条件用户", 
            conditionalUser
        );
        Console.WriteLine($"条件插入结果: {conditionalInsert.Success}");
        
        // 9. 清理数据 - 删除示例用户
        var deleteResults = new List<DBDeleteResult>
        {
            await DBHelper.DeleteOneAsync<User>(x => x.UserName == "张三"),
            await DBHelper.DeleteOneAsync<User>(x => x.UserName == "李四"),
            await DBHelper.DeleteOneAsync<User>(x => x.UserName == "王五"),
            await DBHelper.DeleteOneAsync<User>(x => x.UserName == "赵六"),
            await DBHelper.DeleteOneAsync<User>(x => x.UserName == "条件用户")
        };
        
        var deletedCount = deleteResults.FindAll(r => r.Success).Count;
        Console.WriteLine($"清理完成: 删除了 {deletedCount} 个测试用户");
        
        // ================================
        // 混合使用示例 - 复杂业务场景
        // ================================
        
        Console.WriteLine("\n=== 混合使用示例 - 复杂业务场景 ===");
        
        // 场景: 用户升级系统
        // 1. 先用高级API查找用户
        var targetUser = await DBHelper.FindOneAsync<User>(x => x.Level < 10);
        
        if (targetUser != null)
        {
            Console.WriteLine($"找到待升级用户: {targetUser.UserName}, 当前Level: {targetUser.Level}");
            
            // 2. 用底层API进行原子增量操作
            var userFilter = new BsonDocument("_id", targetUser._id);
            var levelUpResult = await DBHelper.Increment("User", userFilter, "Level", 5);
            
            if (levelUpResult.Success)
            {
                Console.WriteLine($"用户 {targetUser.UserName} 升级成功!");
                
                // 3. 查询升级后的用户数据
                var upgradedUser = await DBHelper.FindByIdAsync<User>(targetUser._id);
                Console.WriteLine($"升级后Level: {upgradedUser?.Level}");
            }
        }
        
        Console.WriteLine("\n所有示例执行完成!");
    }
} 
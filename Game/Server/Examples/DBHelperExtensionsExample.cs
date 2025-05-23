using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ET.DBProxy;
using MongoDB.Bson;

namespace ET.Examples;

/// <summary>
/// DBHelper高级API使用示例 - 类似MongoDB官方驱动的易用接口
/// </summary>
public static class DBHelperExtensionsExample
{
    /// <summary>
    /// 基础CRUD操作示例
    /// </summary>
    public static async Task BasicCrudExample()
    {
        try
        {
            // ========== 插入操作 ==========
            
            // 插入单个用户 - 自动推断集合名为"User"
            var user = new User { Name = "张三", Age = 25, Email = "zhangsan@example.com" };
            var insertResult = await DB.InsertOneAsync(user);
            
            if (insertResult.Success)
            {
                Log.Info($"用户插入成功，ID: {insertResult.InsertedId}");
            }

            // 批量插入用户
            var users = new List<User>
            {
                new User { Name = "李四", Age = 30, Email = "lisi@example.com" },
                new User { Name = "王五", Age = 28, Email = "wangwu@example.com" }
            };
            var insertResults = await DB.InsertManyAsync(users);
            Log.Info($"批量插入完成，成功数量: {insertResults.Count(r => r.Success)}");

            // ========== 查询操作 ==========
            
            // 使用Lambda表达式查询 - 类似于LINQ
            var foundUser = await DB.FindOneAsync<User>(x => x.Name == "张三");
            if (foundUser != null)
            {
                Log.Info($"找到用户: {foundUser.Name}, 年龄: {foundUser.Age}");
            }

            // 根据ID查询
            var userById = await DB.FindByIdAsync<User>("some-user-id");

            // 查询多个用户
            var youngUsers = await DB.FindAsync<User>(x => x.Age < 30);
            Log.Info($"年龄小于30的用户数量: {youngUsers.Count}");

            // 查询所有用户
            var allUsers = await DB.FindAllAsync<User>();
            Log.Info($"所有用户数量: {allUsers.Count}");

            // 分页查询
            var sort = new BsonDocument("Age", 1); // 按年龄升序
            var pagedUsers = await DB.FindAsync<User>(x => x.Age >= 18, skip: 0, limit: 10, sort);
            Log.Info($"分页查询结果数量: {pagedUsers.Count}");

            // ========== 更新操作 ==========
            
            // 根据ID更新
            foundUser.Age = 26;
            var updateResult = await DB.UpdateByIdAsync("user-id", foundUser);
            if (updateResult.Success)
            {
                Log.Info($"更新成功，修改文档数: {updateResult.ModifiedCount}");
            }

            // 根据条件更新
            var updatedUser = new User { Name = "张三", Age = 27, Email = "zhangsan_new@example.com" };
            var updateResult2 = await DB.UpdateOneAsync<User>(x => x.Name == "张三", updatedUser);

            // 部分字段更新
            var updateFields = new BsonDocument 
            { 
                { "Age", 28 }, 
                { "Email", "zhangsan_updated@example.com" } 
            };
            var updateResult3 = await DB.UpdateOneAsync<User>(x => x.Name == "张三", updateFields);

            // 查找并更新（原子操作）
            var findAndUpdatedUser = await DB.FindOneAndUpdateAsync<User>(
                x => x.Name == "张三", 
                new User { Name = "张三", Age = 29, Email = "zhangsan_final@example.com" },
                returnUpdatedDocument: true
            );

            // ========== 删除操作 ==========
            
            // 根据ID删除
            var deleteResult = await DB.DeleteByIdAsync<User>("user-id");
            if (deleteResult.Success)
            {
                Log.Info($"删除成功，删除文档数: {deleteResult.DeletedCount}");
            }

            // 根据条件删除
            var deleteResult2 = await DB.DeleteOneAsync<User>(x => x.Name == "李四");

            // ========== 统计操作 ==========
            
            // 统计文档数量
            var userCount = await DB.CountDocumentsAsync<User>(x => x.Age >= 18);
            Log.Info($"成年用户数量: {userCount}");

            // 检查是否存在
            var exists = await DB.ExistsAsync<User>(x => x.Email == "zhangsan@example.com");
            Log.Info($"邮箱存在: {exists}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "数据库操作示例失败");
        }
    }

    /// <summary>
    /// 高级查询示例
    /// </summary>
    public static async Task AdvancedQueryExample()
    {
        try
        {
            // 复杂条件查询
            var activeUsers = await DB.FindAsync<User>(x => 
                x.Age >= 18 && 
                x.Age <= 65 && 
                x.Email.Contains("@example.com"));

            // 排序和分页
            var sortByAge = new BsonDocument("Age", -1); // 按年龄降序
            var topUsers = await DB.FindAsync<User>(
                x => x.Age >= 25, 
                skip: 0, 
                limit: 5, 
                sort: sortByAge
            );

            // 统计和聚合
            var adultCount = await DB.CountDocumentsAsync<User>(x => x.Age >= 18);
            var seniorCount = await DB.CountDocumentsAsync<User>(x => x.Age >= 60);

            Log.Info($"成年用户: {adultCount}, 老年用户: {seniorCount}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "高级查询示例失败");
        }
    }

    /// <summary>
    /// 游戏特定场景示例
    /// </summary>
    public static async Task GameScenarioExample()
    {
        try
        {
            // ========== 玩家登录场景 ==========
            
            // 查询玩家账号
            var account = await DB.FindOneAsync<Account>(x => x.UserName == "player123");
            
            if (account == null)
            {
                // 创建新账号
                var newAccount = new Account 
                { 
                    UserName = "player123", 
                    Uid = await GenerateUidAsync(),
                    CreateTime = DateTime.UtcNow
                };
                
                var result = await DB.InsertOneAsync(newAccount);
                if (result.Success)
                {
                    Log.Info($"新玩家账号创建成功: {newAccount.UserName}");
                }
            }

            // ========== 游戏数据查询 ==========
            
            // 查询玩家的所有角色
            var playerCharacters = await DB.FindAsync<Character>(x => x.AccountId == account.Uid);
            
            // 查询在线玩家
            var onlinePlayers = await DB.FindAsync<Player>(x => x.IsOnline == true);
            
            // 查询等级排行榜前10名
            var levelSort = new BsonDocument("Level", -1);
            var topPlayers = await DB.FindAsync<Player>(
                x => x.Level > 0, 
                skip: 0, 
                limit: 10, 
                sort: levelSort
            );

            // ========== 游戏数据更新 ==========
            
            // 玩家升级
            var player = await DB.FindOneAsync<Player>(x => x.Id == "player-id");
            if (player != null)
            {
                player.Level++;
                player.Experience = 0;
                await DB.UpdateByIdAsync(player.Id, player);
            }

            // 批量更新在线状态
            var logoutUpdate = new BsonDocument("IsOnline", false);
            // 注意：这里演示单个更新，实际可能需要批量更新的API
            
        }
        catch (Exception ex)
        {
            Log.Error(ex, "游戏场景示例失败");
        }
    }

    /// <summary>
    /// 错误处理示例
    /// </summary>
    public static async Task ErrorHandlingExample()
    {
        try
        {
            // 尝试查询不存在的用户
            var user = await DB.FindOneAsync<User>(x => x.Name == "不存在的用户");
            if (user == null)
            {
                Log.Info("用户不存在，这是正常情况");
            }

            // 尝试插入重复数据（如果有唯一索引）
            var duplicateUser = new User { Name = "重复用户", Email = "duplicate@example.com" };
            
            try
            {
                await DB.InsertOneAsync(duplicateUser);
            }
            catch (Exception ex)
            {
                Log.Warning($"插入重复数据失败: {ex.Message}");
            }

            // 检查操作结果
            var updateResult = await DB.UpdateOneAsync<User>(
                x => x.Name == "不存在的用户", 
                new User { Name = "新名字" }
            );
            
            if (!updateResult.Success || updateResult.ModifiedCount == 0)
            {
                Log.Warning("更新操作未找到匹配的文档");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "错误处理示例中发生异常");
        }
    }

    /// <summary>
    /// 生成UID的辅助方法
    /// </summary>
    private static async Task<int> GenerateUidAsync()
    {
        // 这里可以使用原有的UID生成逻辑
        // 或者实现其他UID生成策略
        return new Random().Next(10000, 99999);
    }
}

// 示例实体类
public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
    public DateTime CreateTime { get; set; }
}

public class Account
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public int Uid { get; set; }
    public DateTime CreateTime { get; set; }
}

public class Character
{
    public string Id { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
}

public class Player
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public bool IsOnline { get; set; }
} 
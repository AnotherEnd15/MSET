using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using ET.DBProxy;

namespace ET.Examples;

/// <summary>
/// DBProxy跨进程操作示例
/// </summary>
public static class DBProxyAsyncExample
{
    /// <summary>
    /// 演示基本的CRUD操作
    /// </summary>
    public static async Task BasicCrudExample()
    {
        try
        {
            // 1. 插入操作
            var user = new { Name = "张三", Age = 25, Email = "zhangsan@example.com" };
            var insertResult = await DBHelper.Insert("user-insert-001", "users", user);
            
            if (insertResult.Success)
            {
                Log.Info($"用户插入成功，ID: {insertResult.InsertedId}");
            }
            else
            {
                Log.Error("用户插入失败");
            }

            // 2. 查询操作
            var filter = new BsonDocument("Name", "张三");
            var foundUser = await DBHelper.FindOne<BsonDocument>("user-find-001", "users", filter);
            
            if (foundUser != null)
            {
                Log.Info($"找到用户: {foundUser.ToJson()}");
            }

            // 3. 更新操作
            var updateFields = new BsonDocument("Age", 26);
            var updateResult = await DBHelper.UpdatePartial("user-update-001", "users", filter, updateFields);
            
            if (updateResult.Success)
            {
                Log.Info($"更新了 {updateResult.ModifiedCount} 个文档");
            }

            // 4. 计数操作
            var countResult = await DBHelper.Count("user-count-001", "users", new BsonDocument());
            Log.Info($"用户总数: {countResult.Count}");

            // 5. 删除操作
            var deleteResult = await DBHelper.Delete("user-delete-001", "users", filter);
            
            if (deleteResult.Success)
            {
                Log.Info($"删除了 {deleteResult.DeletedCount} 个文档");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "数据库操作过程中发生错误");
        }
    }

    /// <summary>
    /// 演示直接使用DBHelper的跨进程操作
    /// </summary>
    public static async Task DirectDBHelperExample()
    {
        try
        {
            // 创建商品数据
            var product = new { 
                Name = "商品A", 
                Price = 99.99, 
                Category = "电子产品",
                CreatedAt = DateTime.UtcNow
            };

            // 插入商品
            var insertResult = await DBHelper.Insert("product-insert-001", "products", product);

            if (insertResult.Success)
            {
                Log.Info($"商品插入成功: {insertResult.InsertedId}");
            }
            else
            {
                Log.Error("商品插入失败");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "直接DBHelper操作失败");
        }
    }

    /// <summary>
    /// 演示并发操作
    /// </summary>
    public static async Task ConcurrentOperationsExample()
    {
        try
        {
            // 创建多个并发插入任务
            var tasks = new Task[10];
            
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks[i] = Task.Run(async () =>
                {
                    var document = new { 
                        Name = $"并发用户{index}", 
                        Index = index, 
                        Timestamp = DateTime.UtcNow 
                    };
                    
                    var result = await DBHelper.Insert($"concurrent-insert-{index}", "concurrent_users", document);
                    Log.Info($"并发插入 {index}: {(result.Success ? "成功" : "失败")}");
                });
            }

            // 等待所有任务完成
            await Task.WhenAll(tasks);
            Log.Info("所有并发插入任务完成");

            // 检查插入结果
            var countResult = await DBHelper.Count("concurrent-count", "concurrent_users", new BsonDocument());
            Log.Info($"并发插入后的用户总数: {countResult.Count}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "并发操作失败");
        }
    }

    /// <summary>
    /// 演示聚合查询
    /// </summary>
    public static async Task AggregateExample()
    {
        try
        {
            // 聚合管道：按类别分组并计算平均价格
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$Category" },
                    { "avgPrice", new BsonDocument("$avg", "$Price") },
                    { "count", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("avgPrice", -1))
            };

            var aggregateResult = await DBHelper.Aggregate<BsonDocument>("aggregate-001", "products", pipeline);
            
            Log.Info($"聚合查询结果数量: {aggregateResult.Count}");
            foreach (var doc in aggregateResult)
            {
                Log.Info($"聚合结果: {doc.ToJson()}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "聚合查询失败");
        }
    }

    /// <summary>
    /// 演示分页查询
    /// </summary>
    public static async Task PagedQueryExample()
    {
        try
        {
            var filter = new BsonDocument(); // 查询所有
            var sort = new BsonDocument("CreatedAt", -1); // 按创建时间倒序

            // 第一页
            var page1 = await DBHelper.FindPaged<BsonDocument>("paged-query-1", "users", filter, 0, 10, sort);
            Log.Info($"第一页结果数量: {page1.Count}");

            // 第二页
            var page2 = await DBHelper.FindPaged<BsonDocument>("paged-query-2", "users", filter, 10, 10, sort);
            Log.Info($"第二页结果数量: {page2.Count}");
            Log.Info($"第二页是否还有更多: {page2.HasMore}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "分页查询失败");
        }
    }

    /// <summary>
    /// 演示原子增量操作
    /// </summary>
    public static async Task IncrementExample()
    {
        try
        {
            var filter = new BsonDocument("Name", "商品A");
            
            // 增加浏览次数
            var incrementResult = await DBHelper.Increment("increment-001", "products", filter, "ViewCount", 1);
            
            if (incrementResult.Success)
            {
                Log.Info($"成功增加浏览次数，修改了 {incrementResult.ModifiedCount} 个文档");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "增量操作失败");
        }
    }

    /// <summary>
    /// 演示条件插入
    /// </summary>
    public static async Task ConditionalInsertExample()
    {
        try
        {
            var filter = new BsonDocument("Email", "unique@example.com");
            var user = new { 
                Name = "唯一用户", 
                Email = "unique@example.com", 
                CreatedAt = DateTime.UtcNow 
            };

            // 只有当邮箱不存在时才插入
            var insertResult = await DBHelper.InsertIfNotExists("conditional-insert-001", "users", filter, user);
            
            if (insertResult.Success)
            {
                Log.Info("用户插入成功（邮箱唯一）");
            }
            else
            {
                Log.Info("用户已存在，未插入");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "条件插入失败");
        }
    }
} 
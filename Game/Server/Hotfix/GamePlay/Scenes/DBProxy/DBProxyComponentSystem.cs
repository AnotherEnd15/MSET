using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ET.DBProxy;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ET.DBProxy;

public static class DBProxyComponentSystem
{
    public static void Init(this DBProxyComponent self, string connectionString, string databaseName)
    {
        self._logger = Log.GetLogger();
        self._client = new MongoClient(connectionString);
        self._database = self._client.GetDatabase(databaseName);

        // 初始化队列
        self._queues = new Queue<MongoTask>[self._queueCount];
        self._queueLocks = new object[self._queueCount];
        self._queueProcessing = new bool[self._queueCount];
        
        for (int i = 0; i < self._queueCount; i++)
        {
            self._queues[i] = new Queue<MongoTask>();
            self._queueLocks[i] = new object();
            self._queueProcessing[i] = false;
        }
        
        self._lastWarningTime = 0;
        
        // 初始化异步结果等待管理
        self._pendingTasks = new System.Collections.Concurrent.ConcurrentDictionary<long, TaskCompletionSource<TaskResult>>();
        
        // 启动超时清理任务
        self.StartTimeoutCleanupTask().Coroutine();
    }

    private static async ETTask StartTimeoutCleanupTask(this DBProxyComponent self)
    {
        while (true)
        {
            try
            {
                await TimerComponent.Instance.WaitAsync(5000); // 每5秒检查一次
                self.CleanupTimeoutTasks();
            }
            catch (Exception ex)
            {
                self._logger.Error(ex, "清理超时任务时发生错误");
            }
        }
    }

    private static void CleanupTimeoutTasks(this DBProxyComponent self)
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var timeoutTasks = new List<long>();

        foreach (var kvp in self._pendingTasks)
        {
            var task = kvp.Value;
            if (task.Task.IsCompleted)
            {
                timeoutTasks.Add(kvp.Key);
            }
            else
            {
                // 检查是否超时（这里简化处理，实际可能需要记录任务创建时间）
                // 为了简单起见，我们依赖Task的CancellationToken或其他机制
            }
        }

        foreach (var taskId in timeoutTasks)
        {
            self._pendingTasks.TryRemove(taskId, out _);
        }
    }

    private static int GetQueueIndex(this DBProxyComponent self, long id)
    {
        return (int)(Math.Abs(id) % self._queueCount);
    }

    public static bool EnqueueOperation(this DBProxyComponent self, MongoTask task)
    {
        if (task.Id == 0)
        {
            task.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        int queueIndex = self.GetQueueIndex(task.Id);
        var queue = self._queues[queueIndex];
        var queueLock = self._queueLocks[queueIndex];

        lock (queueLock)
        {
            if (queue.Count >= self._maxQueueSize)
            {
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (currentTime - self._lastWarningTime > self._warningCooldown)
                {
                    self._logger.Warning($"队列已达预设上限，请监控DBProxy所占用的内存, ID: {task.Id}, 类型: {task.OperationType}");
                    self._lastWarningTime = currentTime;
                }
                return false; // 队列满时直接返回false，不抛异常
            }

            try
            {
                queue.Enqueue(task);
                
                // 尝试驱动队列处理
                self.TryDriveQueue(queueIndex).Coroutine();
                
                return true;
            }
            catch (Exception ex)
            {
                self._logger.Error(ex, $"添加任务到队列时发生错误: {ex.Message}");
                return false;
            }
        }
    }

    private static async ETTask TryDriveQueue(this DBProxyComponent self, int queueIndex)
    {
        var queueLock = self._queueLocks[queueIndex];
        
        lock (queueLock)
        {
            // 如果已经在处理，则不重复驱动
            if (self._queueProcessing[queueIndex])
            {
                return;
            }
            self._queueProcessing[queueIndex] = true;
        }

        try
        {
            // 开始while循环处理队列
            await self.ProcessQueueLoop(queueIndex);
        }
        finally
        {
            lock (queueLock)
            {
                self._queueProcessing[queueIndex] = false;
            }
        }
    }

    private static async ETTask ProcessQueueLoop(this DBProxyComponent self, int queueIndex)
    {
        var queue = self._queues[queueIndex];
        var queueLock = self._queueLocks[queueIndex];

        while (true)
        {
            MongoTask task = null;
            
            lock (queueLock)
            {
                if (queue.Count == 0)
                {
                    break; // 没有任务时自动退出
                }
                task = queue.Dequeue();
            }

            if (task != null)
            {
                try
                {
                    var result = await self.ExecuteMongoOperation(task);
                    
                    // 设置任务结果
                    self.SetTaskResult(task.Id, new TaskResult
                    {
                        Success = true,
                        Data = result
                    });
                }
                catch (Exception ex)
                {
                    self._logger.Error(ex, $"处理MongoDB任务时发生错误: {ex.Message}, ID: {task.Id}");
                    
                    // 设置任务错误结果
                    self.SetTaskResult(task.Id, new TaskResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        Exception = ex
                    });
                }
            }
        }
    }

    public static async Task<BsonDocument> ExecuteMongoOperation(this DBProxyComponent self, MongoTask task)
    {
        var collection = self._database.GetCollection<BsonDocument>(task.CollectionName);

        try
        {
            switch (task.OperationType)
            {
                case MongoOperationType.Insert:
                    await collection.InsertOneAsync(task.Document);
                    return new BsonDocument(DBFieldNames.Inserted, true);

                case MongoOperationType.Update:
                    var updateResult = await collection.UpdateOneAsync(task.Filter, task.Document);
                    return new BsonDocument
                    {
                        { DBFieldNames.MatchedCount, updateResult.MatchedCount },
                        { DBFieldNames.ModifiedCount, updateResult.ModifiedCount }
                    };

                case MongoOperationType.Delete:
                    var deleteResult = await collection.DeleteOneAsync(task.Filter);
                    return new BsonDocument(DBFieldNames.DeletedCount, deleteResult.DeletedCount);

                case MongoOperationType.Find:
                    var findResult = await collection.Find(task.Filter).FirstOrDefaultAsync();
                    return findResult ?? new BsonDocument();

                case MongoOperationType.FindMany:
                    var findManyResult = await collection.Find(task.Filter).ToListAsync();
                    var resultArray = new BsonArray();
                    foreach (var doc in findManyResult)
                    {
                        resultArray.Add(doc);
                    }
                    return new BsonDocument(DBFieldNames.Documents, resultArray);

                case MongoOperationType.Aggregate:
                    var aggregateResult = await collection.Aggregate<BsonDocument>(task.Pipeline).ToListAsync();
                    var aggregateArray = new BsonArray();
                    foreach (var doc in aggregateResult)
                    {
                        aggregateArray.Add(doc);
                    }
                    return new BsonDocument(DBFieldNames.Results, aggregateArray);

                case MongoOperationType.RunCommand:
                    var commandResult = await self._database.RunCommandAsync<BsonDocument>(task.Document);
                    return commandResult;

                case MongoOperationType.ConditionalUpdate:
                    var updateOptions = new UpdateOptions { IsUpsert = false };
                    var conditionalUpdateResult = await collection.UpdateOneAsync(task.Filter, task.Document, updateOptions);
                    return new BsonDocument
                    {
                        { DBFieldNames.MatchedCount, conditionalUpdateResult.MatchedCount },
                        { DBFieldNames.ModifiedCount, conditionalUpdateResult.ModifiedCount }
                    };

                case MongoOperationType.FindOneAndUpdate:
                    var findAndUpdateOptions = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>
                    {
                        ReturnDocument = task.Options?[DBFieldNames.ReturnUpdatedDocument].AsBoolean == true
                            ? ReturnDocument.After
                            : ReturnDocument.Before
                    };
                    var findAndUpdateResult = await collection.FindOneAndUpdateAsync(
                        task.Filter, task.Document, findAndUpdateOptions);
                    return findAndUpdateResult ?? new BsonDocument();

                case MongoOperationType.InsertIfNotExists:
                    var existingDoc = await collection.Find(task.Filter).FirstOrDefaultAsync();
                    bool inserted = false;
                    if (existingDoc == null)
                    {
                        await collection.InsertOneAsync(task.Document);
                        inserted = true;
                    }
                    return new BsonDocument(DBFieldNames.Inserted, inserted);

                default:
                    throw new NotSupportedException($"不支持的操作类型: {task.OperationType}");
            }
        }
        catch (Exception ex)
        {
            self._logger.Error(ex,
                $"执行MongoDB操作时发生错误: {ex.Message}, 操作类型: {task.OperationType}, 集合: {task.CollectionName}");
            throw;
        }
    }

    /// <summary>
    /// 注册等待任务结果
    /// </summary>
    public static Task<TaskResult> RegisterTaskAsync(this DBProxyComponent self, long taskId, int timeoutMs = 0)
    {
        if (timeoutMs <= 0)
            timeoutMs = self._taskTimeoutMs;

        var tcs = new TaskCompletionSource<TaskResult>();
        
        // 设置超时
        var cts = new CancellationTokenSource(timeoutMs);
        cts.Token.Register(() =>
        {
            if (self._pendingTasks.TryRemove(taskId, out _))
            {
                tcs.TrySetResult(new TaskResult
                {
                    Success = false,
                    ErrorMessage = $"任务超时: {taskId}"
                });
            }
        });

        self._pendingTasks[taskId] = tcs;
        return tcs.Task;
    }

    /// <summary>
    /// 设置任务结果
    /// </summary>
    public static void SetTaskResult(this DBProxyComponent self, long taskId, TaskResult result)
    {
        if (self._pendingTasks.TryRemove(taskId, out var tcs))
        {
            tcs.TrySetResult(result);
        }
    }

    /// <summary>
    /// 将MongoTask加入队列等待处理
    /// </summary>
    public static bool EnqueueMongoTask(this DBProxyComponent self, MongoTask task)
    {
        return self.EnqueueOperation(task);
    }

    /// <summary>
    /// 异步执行MongoTask并等待结果
    /// </summary>
    public static async Task<TaskResult> ExecuteMongoTaskAsync(this DBProxyComponent self, MongoTask task, int timeoutMs = 0)
    {
        if (task.Id == 0)
        {
            task.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        // 注册等待任务
        var resultTask = self.RegisterTaskAsync(task.Id, timeoutMs);

        // 入队执行
        bool enqueued = self.EnqueueMongoTask(task);
        if (!enqueued)
        {
            // 入队失败，移除等待任务
            self._pendingTasks.TryRemove(task.Id, out _);
            return new TaskResult
            {
                Success = false,
                ErrorMessage = "队列已满，无法入队"
            };
        }

        // 等待结果
        return await resultTask;
    }
}
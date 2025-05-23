using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDBOperations
{
    /// <summary>
    /// MongoDB操作封装类
    /// </summary>
    public class MongoDBWrapper
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDBWrapper> _logger;
        private readonly ConcurrentDictionary<int, BlockingCollection<MongoTask>> _queues;
        private readonly int _queueCount = 100;
        private readonly int _maxQueueSize = 1000;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<Task> _workerTasks;

        public MongoDBWrapper(string connectionString, string databaseName, ILogger<MongoDBWrapper> logger)
        {
            _logger = logger;
            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(databaseName);
            
            // 初始化队列
            _queues = new ConcurrentDictionary<int, BlockingCollection<MongoTask>>();
            for (int i = 0; i < _queueCount; i++)
            {
                _queues[i] = new BlockingCollection<MongoTask>(_maxQueueSize);
            }
            
            // 启动工作任务
            _cancellationTokenSource = new CancellationTokenSource();
            _workerTasks = new List<Task>();
            StartWorkers();
        }

        private void StartWorkers()
        {
            for (int i = 0; i < _queueCount; i++)
            {
                int queueIndex = i;
                _workerTasks.Add(Task.Run(() => ProcessQueue(queueIndex, _cancellationTokenSource.Token)));
            }
        }

        private async Task ProcessQueue(int queueIndex, CancellationToken cancellationToken)
        {
            var queue = _queues[queueIndex];
            
            while (!cancellationToken.IsCancellationRequested)
            {
                MongoTask task = null;
                try
                {
                    task = queue.Take(cancellationToken);
                    await ExecuteMongoOperation(task);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"处理MongoDB任务时发生错误: {ex.Message}");
                    task?.ErrorCallback?.Invoke(ex);
                }
            }
        }

        private async Task ExecuteMongoOperation(MongoTask task)
        {
            var collection = _database.GetCollection<BsonDocument>(task.CollectionName);
            
            try
            {
                switch (task.OperationType)
                {
                    case MongoOperationType.Insert:
                        await collection.InsertOneAsync(task.Document);
                        task.Callback?.Invoke(new BsonDocument("Inserted", true));
                        break;
                        
                    case MongoOperationType.Update:
                        var updateResult = await collection.UpdateOneAsync(task.Filter, task.Document);
                        task.Callback?.Invoke(new BsonDocument
                        {
                            { "MatchedCount", updateResult.MatchedCount },
                            { "ModifiedCount", updateResult.ModifiedCount }
                        });
                        break;
                        
                    case MongoOperationType.Delete:
                        var deleteResult = await collection.DeleteOneAsync(task.Filter);
                        task.Callback?.Invoke(new BsonDocument("DeletedCount", deleteResult.DeletedCount));
                        break;
                        
                    case MongoOperationType.Find:
                        var findResult = await collection.Find(task.Filter).FirstOrDefaultAsync();
                        task.Callback?.Invoke(findResult ?? new BsonDocument());
                        break;
                        
                    case MongoOperationType.FindMany:
                        var findManyResult = await collection.Find(task.Filter).ToListAsync();
                        var resultArray = new BsonArray();
                        foreach (var doc in findManyResult)
                        {
                            resultArray.Add(doc);
                        }
                        task.Callback?.Invoke(new BsonDocument("Documents", resultArray));
                        break;
                        
                    case MongoOperationType.Aggregate:
                        var aggregateResult = await collection.Aggregate<BsonDocument>(task.Pipeline).ToListAsync();
                        var aggregateArray = new BsonArray();
                        foreach (var doc in aggregateResult)
                        {
                            aggregateArray.Add(doc);
                        }
                        task.Callback?.Invoke(new BsonDocument("Results", aggregateArray));
                        break;
                        
                    case MongoOperationType.RunCommand:
                        var commandResult = await _database.RunCommandAsync<BsonDocument>(task.Document);
                        task.Callback?.Invoke(commandResult);
                        break;
                        
                    case MongoOperationType.ConditionalUpdate:
                        var updateOptions = new UpdateOptions { IsUpsert = false };
                        var conditionalUpdateResult = await collection.UpdateOneAsync(task.Filter, task.Document, updateOptions);
                        task.Callback?.Invoke(new BsonDocument
                        {
                            { "MatchedCount", conditionalUpdateResult.MatchedCount },
                            { "ModifiedCount", conditionalUpdateResult.ModifiedCount }
                        });
                        break;
                    
                    case MongoOperationType.FindOneAndUpdate:
                        var findAndUpdateOptions = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>
                        {
                            ReturnDocument = task.Options?["returnUpdatedDocument"].AsBoolean == true 
                                ? ReturnDocument.After 
                                : ReturnDocument.Before
                        };
                        var findAndUpdateResult = await collection.FindOneAndUpdateAsync(
                            task.Filter, task.Document, findAndUpdateOptions);
                        task.Callback?.Invoke(findAndUpdateResult ?? new BsonDocument());
                        break;
                    
                    case MongoOperationType.InsertIfNotExists:
                        var existingDoc = await collection.Find(task.Filter).FirstOrDefaultAsync();
                        bool inserted = false;
                        if (existingDoc == null)
                        {
                            await collection.InsertOneAsync(task.Document);
                            inserted = true;
                        }
                        task.Callback?.Invoke(new BsonDocument("Inserted", inserted));
                        break;
                        
                    default:
                        throw new NotSupportedException($"不支持的操作类型: {task.OperationType}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"执行MongoDB操作时发生错误: {ex.Message}, 操作类型: {task.OperationType}, 集合: {task.CollectionName}");
                task.ErrorCallback?.Invoke(ex);
                throw;
            }
        }

        private int GetQueueIndex(string id)
        {
            return Math.Abs(id.GetHashCode()) % _queueCount;
        }

        public bool EnqueueOperation(MongoTask task)
        {
            if (string.IsNullOrEmpty(task.Id))
            {
                task.Id = Guid.NewGuid().ToString();
            }
            
            int queueIndex = GetQueueIndex(task.Id);
            var queue = _queues[queueIndex];
            
            if (queue.Count >= _maxQueueSize)
            {
                _logger.LogWarning($"队列 {queueIndex} 已满，无法添加操作 ID: {task.Id}, 类型: {task.OperationType}");
                return false;
            }
            
            try
            {
                queue.Add(task);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"添加任务到队列时发生错误: {ex.Message}");
                return false;
            }
        }

        // 基础CRUD方法
        public Task<BsonDocument> InsertDocument(string id, string collectionName, BsonDocument document)
        {
            var tcs = new TaskCompletionSource<BsonDocument>();
            
            bool queued = EnqueueOperation(new MongoTask
            {
                Id = id,
                OperationType = MongoOperationType.Insert,
                CollectionName = collectionName,
                Document = document,
                Callback = result => tcs.TrySetResult(result),
                ErrorCallback = ex => tcs.TrySetException(ex)
            });
            
            if (!queued)
            {
                tcs.TrySetException(new InvalidOperationException("队列已满，无法添加操作"));
            }
            
            return tcs.Task;
        }

        public Task<BsonDocument> UpdateDocument(string id, string collectionName, BsonDocument filter, BsonDocument update)
        {
            var tcs = new TaskCompletionSource<BsonDocument>();
            
            bool queued = EnqueueOperation(new MongoTask
            {
                Id = id,
                OperationType = MongoOperationType.Update,
                CollectionName = collectionName,
                Filter = filter,
                Document = new BsonDocument("$set", update),
                Callback = result => tcs.TrySetResult(result),
                ErrorCallback = ex => tcs.TrySetException(ex)
            });
            
            if (!queued)
            {
                tcs.TrySetException(new InvalidOperationException("队列已满，无法添加操作"));
            }
            
            return tcs.Task;
        }

        public Task<BsonDocument> DeleteDocument(string id, string collectionName, BsonDocument filter)
        {
            var tcs = new TaskCompletionSource<BsonDocument>();
            
            bool queued = EnqueueOperation(new MongoTask
            {
                Id = id,
                OperationType = MongoOperationType.Delete,
                CollectionName = collectionName,
                Filter = filter,
                Callback = result => tcs.TrySetResult(result),
                ErrorCallback = ex => tcs.TrySetException(ex)
            });
            
            if (!queued)
            {
                tcs.TrySetException(new InvalidOperationException("队列已满，无法添加操作"));
            }
            
            return tcs.Task;
        }

        public Task<BsonDocument> FindDocument(string id, string collectionName, BsonDocument filter)
        {
            var tcs = new TaskCompletionSource<BsonDocument>();
            
            bool queued = EnqueueOperation(new MongoTask
            {
                Id = id,
                OperationType = MongoOperationType.Find,
                CollectionName = collectionName,
                Filter = filter,
                Callback = result => tcs.TrySetResult(result),
                ErrorCallback = ex => tcs.TrySetException(ex)
            });
            
            if (!queued)
            {
                tcs.TrySetException(new InvalidOperationException("队列已满，无法添加操作"));
            }
            
            return tcs.Task;
        }

        public Task<BsonDocument> FindManyDocuments(string id, string collectionName, BsonDocument filter)
        {
            var tcs = new TaskCompletionSource<BsonDocument>();
            
            bool queued = EnqueueOperation(new MongoTask
            {
                Id = id,
                OperationType = MongoOperationType.FindMany,
                CollectionName = collectionName,
                Filter = filter,
                Callback = result => tcs.TrySetResult(result),
                ErrorCallback = ex => tcs.TrySetException(ex)
            });
            
            if (!queued)
            {
                tcs.TrySetException(new InvalidOperationException("队列已满，无法添加操作"));
            }
            
            return tcs.Task;
        }

        public Task<BsonDocument> AggregateDocuments(string id, string collectionName, BsonDocument[] pipeline)
        {
            var tcs = new TaskCompletionSource<BsonDocument>();
            
            bool queued = EnqueueOperation(new MongoTask
            {
                Id = id,
                OperationType = MongoOperationType.Aggregate,
                CollectionName = collectionName,
                Pipeline = pipeline,
                Callback = result => tcs.TrySetResult(result),
                ErrorCallback = ex => tcs.TrySetException(ex)
            });
                        if (!queued)
            {
                tcs.TrySetException(new InvalidOperationException("队列已满，无法添加操作"));
            }
            
            return tcs.Task;
        }

        public Task<BsonDocument> RunCommand(string id, BsonDocument command)
        {
            var tcs = new TaskCompletionSource<BsonDocument>();
            
            bool queued = EnqueueOperation(new MongoTask
            {
                Id = id,
                OperationType = MongoOperationType.RunCommand,
                Document = command,
                Callback = result => tcs.TrySetResult(result),
                ErrorCallback = ex => tcs.TrySetException(ex)
            });
            
            if (!queued)
            {
                tcs.TrySetException(new InvalidOperationException("队列已满，无法添加操作"));
            }
            
            return tcs.Task;
        }

        public Task<BsonDocument> ConditionalUpdateDocument(
            string id,
            string collectionName,
            BsonDocument filter,
            BsonDocument update)
        {
            var tcs = new TaskCompletionSource<BsonDocument>();
            
            bool queued = EnqueueOperation(new MongoTask
            {
                Id = id,
                OperationType = MongoOperationType.ConditionalUpdate,
                CollectionName = collectionName,
                Filter = filter,
                Document = new BsonDocument("$set", update),
                Callback = result => tcs.TrySetResult(result),
                ErrorCallback = ex => tcs.TrySetException(ex)
            });
            
            if (!queued)
            {
                tcs.TrySetException(new InvalidOperationException("队列已满，无法添加操作"));
            }
            
            return tcs.Task;
        }

        public Task<BsonDocument> FindOneAndUpdateDocument(
            string id,
            string collectionName,
            BsonDocument filter,
            BsonDocument update,
            bool returnUpdatedDocument = true)
        {
            var tcs = new TaskCompletionSource<BsonDocument>();
            
            bool queued = EnqueueOperation(new MongoTask
            {
                Id = id,
                OperationType = MongoOperationType.FindOneAndUpdate,
                CollectionName = collectionName,
                Filter = filter,
                Document = new BsonDocument("$set", update),
                Options = new BsonDocument("returnUpdatedDocument", returnUpdatedDocument),
                Callback = result => tcs.TrySetResult(result),
                ErrorCallback = ex => tcs.TrySetException(ex)
            });
            
            if (!queued)
            {
                tcs.TrySetException(new InvalidOperationException("队列已满，无法添加操作"));
            }
            
            return tcs.Task;
        }

        public Task<BsonDocument> InsertIfNotExists(
            string id,
            string collectionName,
            BsonDocument filter,
            BsonDocument document)
        {
            var tcs = new TaskCompletionSource<BsonDocument>();
            
            bool queued = EnqueueOperation(new MongoTask
            {
                Id = id,
                OperationType = MongoOperationType.InsertIfNotExists,
                CollectionName = collectionName,
                Filter = filter,
                Document = document,
                Callback = result => tcs.TrySetResult(result),
                ErrorCallback = ex => tcs.TrySetException(ex)
            });
            
            if (!queued)
            {
                tcs.TrySetException(new InvalidOperationException("队列已满，无法添加操作"));
            }
            
            return tcs.Task;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            Task.WaitAll(_workerTasks.ToArray());
            
            foreach (var queue in _queues.Values)
            {
                queue.Dispose();
            }
            
            _cancellationTokenSource.Dispose();
        }
    }
}
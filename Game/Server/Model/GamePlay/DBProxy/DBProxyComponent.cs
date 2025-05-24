using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace ET.DBProxy;

public enum MongoOperationType
{
    Insert,
    Update,
    Delete,
    Find,
    FindMany,
    Aggregate,
    RunCommand,
    ConditionalUpdate,
    FindOneAndUpdate,
    InsertIfNotExists
}

[System.Serializable]
public class MongoTask
{
    /// <summary>
    /// 任务唯一标识符
    /// </summary>
    public long Id { get; set; }
        
    /// <summary>
    /// 操作类型
    /// </summary>
    public MongoOperationType OperationType { get; set; }
        
    /// <summary>
    /// 集合名称
    /// </summary>
    public string CollectionName { get; set; }
        
    /// <summary>
    /// 文档内容
    /// </summary>
    public BsonDocument Document { get; set; }
        
    /// <summary>
    /// 过滤条件
    /// </summary>
    public BsonDocument Filter { get; set; }
        
    /// <summary>
    /// 聚合管道
    /// </summary>
    public BsonDocument[] Pipeline { get; set; }
        
    /// <summary>
    /// 操作选项
    /// </summary>
    public BsonDocument Options { get; set; }
}

/// <summary>
/// 任务执行结果包装
/// </summary>
public class TaskResult
{
    public bool Success { get; set; }
    public BsonDocument Data { get; set; }
    public string ErrorMessage { get; set; }
    public Exception Exception { get; set; }
}

[ComponentOf(typeof(Scene))]
public class DBProxyComponent : Entity, IAwake<string, string>
{
    [StaticField]
    public static DBProxyComponent Instance;
    
    public IMongoClient _client;
    public IMongoDatabase _database;
    public ILogger _logger;
    public Queue<MongoTask>[] _queues;
    public object[] _queueLocks;
    public bool[] _queueProcessing;
    public int _queueCount = 100;
    public int _maxQueueSize = 1000;
    public long _lastWarningTime;
    public int _warningCooldown = 10000; // 10秒CD，以毫秒为单位
    
    // 异步结果等待管理 - key改为long类型
    public ConcurrentDictionary<long, TaskCompletionSource<TaskResult>> _pendingTasks;
    public int _taskTimeoutMs = 30000; // 30秒超时
}
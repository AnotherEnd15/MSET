using ET.DBProxy;
using MongoDB.Bson;

namespace ET;

/// <summary>
/// 数据库操作请求参数 - 跨进程序列化（简化版）
/// </summary>
[System.Serializable]
public class DBOperationRequest
{
    /// <summary>
    /// 操作ID - 用于追踪和异步等待
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 集合名称
    /// </summary>
    public string CollectionName { get; set; }
    
    /// <summary>
    /// 操作类型
    /// </summary>
    public MongoOperationType OperationType { get; set; }
    
    /// <summary>
    /// 通用文档字段 - 用于插入/更新/命令等
    /// </summary>
    public BsonDocument Document { get; set; }
    
    /// <summary>
    /// 查询过滤条件
    /// </summary>
    public BsonDocument Filter { get; set; }
    
    /// <summary>
    /// 聚合管道 - 用于复杂查询、分页、统计等
    /// </summary>
    public BsonDocument[] Pipeline { get; set; }
    
    /// <summary>
    /// 操作选项
    /// </summary>
    public BsonDocument Options { get; set; }
} 
using System;
using MongoDB.Bson;

namespace MongoDBOperations
{
    /// <summary>
    /// MongoDB操作任务
    /// </summary>
    public class MongoTask
    {
        /// <summary>
        /// 任务唯一标识符
        /// </summary>
        public string Id { get; set; }
        
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
        /// 结果回调
        /// </summary>
        public Action<BsonDocument> Callback { get; set; }
        
        /// <summary>
        /// 错误回调
        /// </summary>
        public Action<Exception> ErrorCallback { get; set; }
        
        /// <summary>
        /// 操作选项
        /// </summary>
        public BsonDocument Options { get; set; }
    }
}
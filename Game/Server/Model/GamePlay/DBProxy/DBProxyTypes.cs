using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace ET.DBProxy;

/// <summary>
/// 插入操作结果
/// </summary>
[System.Serializable]
public class DBInsertResult
{
    public bool Success { get; set; }
    public string InsertedId { get; set; }
    public bool IsAcknowledged { get; set; }
    
    public static DBInsertResult FromBsonDocument(BsonDocument doc)
    {
        if (doc == null) return new DBInsertResult { Success = false };
        
        return new DBInsertResult
        {
            Success = doc.GetValue(DBFieldNames.Inserted, false).AsBoolean,
            InsertedId = doc.GetValue(DBFieldNames.InsertedId, "").AsString,
            IsAcknowledged = true
        };
    }
}

/// <summary>
/// 更新操作结果
/// </summary>
[System.Serializable]
public class DBUpdateResult
{
    public long MatchedCount { get; set; }
    public long ModifiedCount { get; set; }
    public bool IsAcknowledged { get; set; }
    public string UpsertedId { get; set; }
    public bool Success => IsAcknowledged && ModifiedCount > 0;
    
    public static DBUpdateResult FromBsonDocument(BsonDocument doc)
    {
        if (doc == null) return new DBUpdateResult { IsAcknowledged = false };
        
        return new DBUpdateResult
        {
            MatchedCount = doc.GetValue(DBFieldNames.MatchedCount, 0).AsInt64,
            ModifiedCount = doc.GetValue(DBFieldNames.ModifiedCount, 0).AsInt64,
            IsAcknowledged = true,
            UpsertedId = doc.GetValue(DBFieldNames.UpsertedId, "").AsString
        };
    }
}

/// <summary>
/// 删除操作结果
/// </summary>
[System.Serializable]
public class DBDeleteResult
{
    public long DeletedCount { get; set; }
    public bool IsAcknowledged { get; set; }
    public bool Success => IsAcknowledged && DeletedCount > 0;
    
    public static DBDeleteResult FromBsonDocument(BsonDocument doc)
    {
        if (doc == null) return new DBDeleteResult { IsAcknowledged = false };
        
        return new DBDeleteResult
        {
            DeletedCount = doc.GetValue(DBFieldNames.DeletedCount, 0).AsInt64,
            IsAcknowledged = true
        };
    }
}

/// <summary>
/// 查找多个文档的结果
/// </summary>
[System.Serializable]
public class DBFindManyResult<T> where T : class
{
    public List<T> Documents { get; set; } = new List<T>();
    public int Count => Documents.Count;
    public bool HasResults => Count > 0;
}

/// <summary>
/// 计数结果
/// </summary>
[System.Serializable]
public class DBCountResult
{
    public long Count { get; set; }
    
    public static DBCountResult FromBsonDocument(BsonDocument doc)
    {
        if (doc == null) return new DBCountResult { Count = 0 };
        
        // 处理聚合计数结果
        if (doc.Contains(DBFieldNames.Results))
        {
            var results = doc[DBFieldNames.Results].AsBsonArray;
            if (results.Count > 0)
            {
                var countDoc = results[0].AsBsonDocument;
                return new DBCountResult
                {
                    Count = countDoc.GetValue(DBFieldNames.Count, 0).AsInt64
                };
            }
        }
        
        return new DBCountResult { Count = 0 };
    }
}

/// <summary>
/// 分页查询结果
/// </summary>
[System.Serializable]
public class DBPagedResult<T> where T : class
{
    public List<T> Documents { get; set; } = new List<T>();
    public int Skip { get; set; }
    public int Limit { get; set; }
    public int Count => Documents.Count;
    public bool HasMore => Count == Limit;
} 
namespace ET.DBProxy;

/// <summary>
/// 数据库操作中使用的BsonDocument字段名常量
/// 统一管理，避免魔法字符串
/// </summary>
public static class DBFieldNames
{
    // 插入操作结果字段
    public const string Inserted = "Inserted";
    public const string InsertedId = "InsertedId";
    
    // 更新操作结果字段
    public const string MatchedCount = "MatchedCount";
    public const string ModifiedCount = "ModifiedCount";
    public const string UpsertedId = "UpsertedId";
    
    // 删除操作结果字段
    public const string DeletedCount = "DeletedCount";
    
    // 聚合查询结果字段
    public const string Results = "Results";
    public const string Documents = "Documents";
    public const string Count = "count";
    
    // 错误信息字段
    public const string Error = "Error";
    
    // MongoDB操作符
    public const string Set = "$set";
    public const string Inc = "$inc";
    public const string Match = "$match";
    public const string Group = "$group";
    public const string Sort = "$sort";
    public const string Skip = "$skip";
    public const string Limit = "$limit";
    
    // 特殊字段
    public const string Id = "_id";
    public const string ReturnUpdatedDocument = "returnUpdatedDocument";
} 
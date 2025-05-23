namespace MongoDBOperations
{
    /// <summary>
    /// MongoDB操作类型枚举
    /// </summary>
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
}
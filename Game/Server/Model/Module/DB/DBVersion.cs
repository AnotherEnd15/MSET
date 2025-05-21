using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public class DBVersion
{
    [BsonId]
    public int Id;
    public DBVersionEnum Version;
}
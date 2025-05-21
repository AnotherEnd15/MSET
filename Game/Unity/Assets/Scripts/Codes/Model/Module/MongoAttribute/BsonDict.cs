
using System;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Options
{
    public enum DictionaryRepresentation
    {
        ArrayOfArrays
    }
}


namespace MongoDB.Bson.Serialization.Attributes
{
    public class BsonDictionaryOptionsAttribute : Attribute
    {
        public BsonDictionaryOptionsAttribute(DictionaryRepresentation representation)
        {
            
        }
    }

    public class BsonElementAttribute : Attribute
    {
        public BsonElementAttribute(string str = "")
        {
            
        }
    }

    public class BsonIgnoreAttribute : Attribute
    {
        
    }

    public class BsonIgnoreIfDefault : Attribute
    {
        
    }

    public class BsonIdAttribute : Attribute
    {
        
    }

    public class BsonIgnoreIfNullAttribute : Attribute
    {
        
    }

    public class BsonDateTimeOptionsAttribute : Attribute
    {
        
    }

    public class BsonRepresentationAttribute : Attribute
    {
        
    }
}
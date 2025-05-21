using System;
using Newtonsoft.Json;

namespace ET
{
    public static class JsonHelper
    {
        [StaticField]
        private static readonly MongoDB.Bson.IO.JsonWriterSettings logDefineSettings = new MongoDB.Bson.IO.JsonWriterSettings() { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson };
 
        
        public static string ToJson(object message)
        {
            return MongoDB.Bson.BsonExtensionMethods.ToJson(message, logDefineSettings);
        }

        public static object FromJson(Type type, string json)
        {
            return MongoHelper.FromJson(type, json);
        }

        public static T FromJson<T>(string json)
        {
            return MongoHelper.FromJson<T>(json);
        }

    }
}
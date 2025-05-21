using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FixMath;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace ET
{
    public static class MongoHelper
    {
      

        [StaticField]
        private static readonly JsonWriterSettings defaultSettings = new()
        {
            OutputMode = JsonOutputMode.RelaxedExtendedJson,
            Indent = true
        };

        [StaticField]
        public static HashSet<Type> TypeBlackList = new() {  };

        static MongoHelper()
        {
            // 自动注册IgnoreExtraElements

            ConventionPack conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };

            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

            RegisterStruct<fp>();
            RegisterStruct<fp2>();

            IEnumerable<Type> types = null;
            if (EventSystem.Instance != null)
                types = EventSystem.Instance.GetTypes().Values;
            else
            {
                throw new Exception("EventSystem还没初始化完毕 不能调用MongoHelper.Init()");
            }
            foreach (Type type in types)
            {
                if (!type.IsSubclassOf(typeof (Object)))
                {
                    continue;
                }

                if (type.IsGenericType)
                {
                    continue;
                }
                
                if(TypeBlackList.Contains(type))
                    continue;

                BsonClassMap.LookupClassMap(type);
            }
            
            var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);
            BsonSerializer.RegisterSerializer(objectSerializer);
            
            var decimalSerializer = new DecimalSerializer(BsonType.Decimal128, new
                    RepresentationConverter(allowOverflow: false, allowTruncation:
                        true)); 
            BsonSerializer.RegisterSerializer(decimalSerializer); 
        }

        public static void Init()
        {
        }

        public static void RegisterStruct<T>() where T : struct
        {
            BsonSerializer.RegisterSerializer(typeof (T), new StructBsonSerialize<T>());
        }

        public static string ToJson(object obj)
        {
            return obj.ToJson(defaultSettings);
        }

        public static string ToJson(object obj, JsonWriterSettings settings)
        {
            return obj.ToJson(settings);
        }

        public static T FromJson<T>(string str)
        {
            try
            {
                return BsonSerializer.Deserialize<T>(str);
            }
            catch (Exception e)
            {
                throw new Exception($"{str}\n{e}");
            }
        }

        public static object FromJson(Type type, string str)
        {
            return BsonSerializer.Deserialize(str, type);
        }

        public static byte[] Serialize(object obj)
        {
            return obj.ToBson();
        }

        public static void Serialize(object message, MemoryStream stream)
        {
            using (BsonBinaryWriter bsonWriter = new BsonBinaryWriter(stream, BsonBinaryWriterSettings.Defaults))
            {
                BsonSerializationContext context = BsonSerializationContext.CreateRoot(bsonWriter);
                BsonSerializationArgs args = default;
                args.NominalType = typeof (object);
                IBsonSerializer serializer = BsonSerializer.LookupSerializer(args.NominalType);
                serializer.Serialize(context, args, message);
            }
        }

        public static object Deserialize(Type type, byte[] bytes)
        {
            try
            {
                return BsonSerializer.Deserialize(bytes, type);
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {type.Name}", e);
            }
        }

        public static object Deserialize(Type type, byte[] bytes, int index, int count)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(bytes, index, count))
                {
                    return BsonSerializer.Deserialize(memoryStream, type);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {type.Name}", e);
            }
        }

        public static object Deserialize(Type type, Stream stream)
        {
            try
            {
                return BsonSerializer.Deserialize(stream, type);
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {type.Name}", e);
            }
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    return (T)BsonSerializer.Deserialize(memoryStream, typeof (T));
                }
            }
            catch (Exception e)
            {
                throw new Exception($"from bson error: {typeof (T).Name}", e);
            }
        }

        public static T Deserialize<T>(byte[] bytes, int index, int count)
        {
            return (T)Deserialize(typeof (T), bytes, index, count);
        }

        public static T Clone<T>(T t)
        {
            return Deserialize<T>(Serialize(t));
        }
    }
}
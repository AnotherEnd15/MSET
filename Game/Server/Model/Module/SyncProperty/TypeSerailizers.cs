// TypeSerializers.cs
using System;
using System.IO;

namespace SyncFramework
{
    public static class TypeSerializers
    {
        // 复杂类型序列化
        public static void Serialize(BinaryWriter writer, object value)
        {
            if (value == null)
            {
                writer.Write(false);
                return;
            }
            
            writer.Write(true);
            
            // 这里可以根据需要扩展更多复杂类型的序列化
            if (value is ISyncable syncable)
            {
                writer.Write(true); // 标记为同步对象
                syncable.SerializeDirtyValues(writer);
            }
            else
            {
                writer.Write(false); // 非同步对象，可以实现其他类型序列化
                throw new NotSupportedException($"不支持序列化的类型: {value.GetType().Name}");
            }
        }
        
        // 复杂类型反序列化
        public static T Deserialize<T>(BinaryReader reader)
        {
            bool hasValue = reader.ReadBoolean();
            if (!hasValue)
                return default(T);
            
            bool isSyncable = reader.ReadBoolean();
            if (isSyncable)
            {
                if (typeof(T) is ISyncable)
                {
                    var result = Activator.CreateInstance<T>();
                    if (result is ISyncable syncable)
                    {
                        syncable.DeserializeValues(reader);
                        return result;
                    }
                }
            }
            
            throw new NotSupportedException($"不支持反序列化的类型: {typeof(T).Name}");
        }
    }
}
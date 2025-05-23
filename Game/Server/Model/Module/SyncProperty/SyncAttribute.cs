// SyncAttributes.cs
using System;
using System.IO;

namespace SyncFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SyncClassAttribute : Attribute {}

    [AttributeUsage(AttributeTargets.Field)]
    public class SyncFieldAttribute : Attribute {}
    
    [AttributeUsage(AttributeTargets.Property)]
    public class SyncPropertyAttribute : Attribute {}
    
    public interface ISyncable
    {
        // 直接序列化脏数据到二进制流
        void SerializeDirtyValues(BinaryWriter writer);
        
        // 直接从二进制流反序列化数据
        void DeserializeValues(BinaryReader reader);
        
        // 清除脏状态
        void ClearDirtyState();
        
        // 是否有脏数据
        bool IsDirty { get; }
        
        // 获取脏数据数量
        int DirtyCount { get; }
    }
}
// // SyncHelper.cs
// using System;
// using System.IO;
//
// namespace SyncFramework
// {
//     public static class SyncHelper
//     {
//         // 序列化单个对象的脏数据
//         public static byte[] SerializeObject(ISyncable syncObject, out int classId)
//         {
//             Type type = syncObject.GetType();
//             string typeName = type.Name;
//             
//             // 获取类ID
//             var classIdField = typeof(SyncMetadata.Ids).GetField($"{typeName}_ClassId");
//             if (classIdField == null)
//                 throw new InvalidOperationException($"类型 {typeName} 未在SyncMetadata中注册");
//                 
//             classId = (int)classIdField.GetValue(null);
//             
//             // 若没有脏数据，返回空数组
//             if (!syncObject.IsDirty)
//                 return new byte[0];
//                 
//             // 序列化脏数据
//             using (MemoryStream ms = new MemoryStream())
//             {
//                 BinaryWriter writer = new BinaryWriter(ms);
//                 syncObject.SerializeDirtyValues(writer);
//                 syncObject.ClearDirtyState();
//                 return ms.ToArray();
//             }
//         }
//         
//         // 反序列化数据到对象
//         public static void DeserializeObject(ISyncable syncObject, byte[] data)
//         {
//             if (data == null || data.Length == 0)
//                 return;
//                 
//             using (MemoryStream ms = new MemoryStream(data))
//             {
//                 BinaryReader reader = new BinaryReader(ms);
//                 syncObject.DeserializeValues(reader);
//             }
//         }
//         
//         // 创建网络消息
//         public static byte[] CreateSyncMessage(ISyncable syncObject)
//         {
//             int classId;
//             byte[] syncData = SerializeObject(syncObject, out classId);
//             
//             // 若没有脏数据，返回空数组
//             if (syncData.Length == 0)
//                 return new byte[0];
//                 
//             using (MemoryStream ms = new MemoryStream())
//             {
//                 BinaryWriter writer = new BinaryWriter(ms);
//                 writer.Write(classId);
//                 writer.Write(syncData.Length);
//                 writer.Write(syncData);
//                 return ms.ToArray();
//             }
//         }
//     }
// }
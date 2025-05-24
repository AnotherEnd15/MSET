using System;
using System.IO;

namespace SyncFramework
{
	public static class SyncHelper
	{
		// 泛型版本的序列化，性能更好
		public static byte[] SerializeObject<T>(T syncObject) where T : class, ISyncable<T>
		{
			// 若没有脏数据，返回空数组
			if (!syncObject.IsDirty)
				return new byte[0];

			// 序列化脏数据
			using (MemoryStream ms = new MemoryStream())
			{
				BinaryWriter writer = new BinaryWriter(ms);
				syncObject.SerializeDirtyValues(writer);
				syncObject.ClearDirtyState();
				return ms.ToArray();
			}
		}

		// 反序列化数据到对象
		public static void DeserializeObject(ISyncable syncObject, byte[] data)
		{
			if (data == null || data.Length == 0)
				return;

			using (MemoryStream ms = new MemoryStream(data))
			{
				BinaryReader reader = new BinaryReader(ms);
				// 跳过ClassId（前面CreateSyncMessage写入的）
				int classId = reader.ReadInt32();
				// 跳过数据长度（前面CreateSyncMessage写入的）
				int dataLength = reader.ReadInt32();
				// 现在读取实际的同步数据
				syncObject.DeserializeValues(reader);
			}
		}

		// 泛型版本的创建网络消息
		public static byte[] CreateSyncMessage<T>(T syncObject) where T : class, ISyncable<T>
		{
			byte[] syncData = SerializeObject(syncObject);

			// 若没有脏数据，返回空数组
			if (syncData.Length == 0)
				return new byte[0];

			using (MemoryStream ms = new MemoryStream())
			{
				BinaryWriter writer = new BinaryWriter(ms);
				writer.Write(syncObject.ClassId);
				writer.Write(syncData.Length);
				writer.Write(syncData);
				return ms.ToArray();
			}
		}
	}

	// 为了处理没有同步定义的情况，提供一个空的元数据实现
	public static partial class SyncMetadata
	{
		public static partial class Ids
		{
			// 空实现，防止编译错误
		}

		static SyncMetadata()
		{
			// 确保在没有生成代码时也能正常工作
			if (ClassNames == null)
			{
				ClassNames = new System.Collections.Generic.Dictionary<int, string>();
			}
			if (Fields == null)
			{
				Fields = new System.Collections.Generic.Dictionary<int, FieldMetadata>();
			}
		}
	}
} 
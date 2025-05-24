using System;
using System.Collections.Generic;
using System.IO;

namespace SyncFramework
{
	public static class CollectionUtils
	{
		// 序列化集合变更记录
		public static void SerializeCollectionChanges(BinaryWriter writer, List<CollectionChange> changes)
		{
			writer.Write(changes.Count);
			foreach (var change in changes)
			{
				writer.Write((byte)change.Operation);
				
				// 序列化Key
				TypeSerializers.SerializeField(writer, change.Key);
				
				// 序列化Value
				TypeSerializers.SerializeField(writer, change.Value);
				
				// 序列化OldValue（仅Replace操作需要）
				if (change.Operation == CollectionOperation.Replace)
				{
					TypeSerializers.SerializeField(writer, change.OldValue);
				}
			}
		}

		// 反序列化集合变更记录
		public static List<CollectionChange> DeserializeCollectionChanges(BinaryReader reader)
		{
			int count = reader.ReadInt32();
			var changes = new List<CollectionChange>(count);
			
			for (int i = 0; i < count; i++)
			{
				var change = new CollectionChange
				{
					Operation = (CollectionOperation)reader.ReadByte(),
					Key = TypeSerializers.DeserializeField<object>(reader),
					Value = TypeSerializers.DeserializeField<object>(reader)
				};
				
				if (change.Operation == CollectionOperation.Replace)
				{
					change.OldValue = TypeSerializers.DeserializeField<object>(reader);
				}
				
				changes.Add(change);
			}
			
			return changes;
		}

		// 应用集合变更到目标集合
		public static void ApplyCollectionChanges<T>(ICollection<T> collection, List<CollectionChange> changes)
		{
			foreach (var change in changes)
			{
				switch (change.Operation)
				{
					case CollectionOperation.Clear:
						collection.Clear();
						break;
						
					case CollectionOperation.Add:
						if (change.Value is T value)
							collection.Add(value);
						break;
						
					case CollectionOperation.Remove:
						if (change.Value is T removeValue)
							collection.Remove(removeValue);
						break;
				}
			}
		}

		// 应用字典变更
		public static void ApplyDictionaryChanges<TKey, TValue>(IDictionary<TKey, TValue> dictionary, List<CollectionChange> changes)
		{
			foreach (var change in changes)
			{
				switch (change.Operation)
				{
					case CollectionOperation.Clear:
						dictionary.Clear();
						break;
						
					case CollectionOperation.Add:
					case CollectionOperation.Replace:
						if (change.Key is TKey key && change.Value is TValue value)
							dictionary[key] = value;
						break;
						
					case CollectionOperation.Remove:
						if (change.Key is TKey removeKey)
							dictionary.Remove(removeKey);
						break;
				}
			}
		}

		// 应用列表变更
		public static void ApplyListChanges<T>(IList<T> list, List<CollectionChange> changes)
		{
			foreach (var change in changes)
			{
				switch (change.Operation)
				{
					case CollectionOperation.Clear:
						list.Clear();
						break;
						
					case CollectionOperation.Add:
						if (change.Value is T value)
						{
							if (change.Key is int index && index >= 0 && index <= list.Count)
								list.Insert(index, value);
							else
								list.Add(value);
						}
						break;
						
					case CollectionOperation.Remove:
						if (change.Key is int removeIndex && removeIndex >= 0 && removeIndex < list.Count)
							list.RemoveAt(removeIndex);
						break;
						
					case CollectionOperation.Replace:
						if (change.Key is int replaceIndex && replaceIndex >= 0 && replaceIndex < list.Count && change.Value is T replaceValue)
							list[replaceIndex] = replaceValue;
						break;
				}
			}
		}
	}
} 
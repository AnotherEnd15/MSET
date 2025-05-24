using System;
using System.Collections.Generic;
using System.IO;
using ET;
using MemoryPack;

namespace SyncFramework
{
	public static class TypeSerializers
	{
		// 类型标识符
		private const byte TYPE_NULL = 0;
		private const byte TYPE_PRIMITIVE = 1;
		private const byte TYPE_COMPLEX = 2;
		private const byte TYPE_COLLECTION_CHANGES = 3;
		private const byte TYPE_SIMPLE_COLLECTION = 4;

		// 基本类型代码（用于动态类型）
		private const byte PRIMITIVE_BOOL = 1;
		private const byte PRIMITIVE_BYTE = 2;
		private const byte PRIMITIVE_SBYTE = 3;
		private const byte PRIMITIVE_CHAR = 4;
		private const byte PRIMITIVE_DECIMAL = 5;
		private const byte PRIMITIVE_DOUBLE = 6;
		private const byte PRIMITIVE_FLOAT = 7;
		private const byte PRIMITIVE_INT = 8;
		private const byte PRIMITIVE_UINT = 9;
		private const byte PRIMITIVE_LONG = 10;
		private const byte PRIMITIVE_ULONG = 11;
		private const byte PRIMITIVE_SHORT = 12;
		private const byte PRIMITIVE_USHORT = 13;
		private const byte PRIMITIVE_STRING = 14;

		// 复杂对象的序列化 - 使用MemoryPack + Brotli
		public static byte[] Serialize(object message)
		{
			return MemoryPackHelper.Serialize(message);
		}

		// 复杂对象的反序列化 - 使用MemoryPack + Brotli
		public static object Deserialize(Type type, byte[] bytes, int index, int count)
		{
			return MemoryPackHelper.Deserialize(type, bytes, index, count);
		}

		// 字段值序列化（用于同步系统）
		public static void SerializeField(BinaryWriter writer, object value)
		{
			if (value == null)
			{
				writer.Write(TYPE_NULL);
				return;
			}

			var type = value.GetType();

			// 基本类型直接序列化
			if (IsPrimitiveType(type))
			{
				writer.Write(TYPE_PRIMITIVE);
				WriteDynamicPrimitiveValue(writer, value, type);
				return;
			}

			// 集合变更记录
			if (value is List<CollectionChange> changes)
			{
				writer.Write(TYPE_COLLECTION_CHANGES);
				CollectionUtils.SerializeCollectionChanges(writer, changes);
				return;
			}

			// 简单集合（集合<基本类型>）
			if (IsSimpleCollection(type))
			{
				writer.Write(TYPE_SIMPLE_COLLECTION);
				SerializeSimpleCollection(writer, value);
				return;
			}

			// 复杂对象使用MemoryPack序列化
			writer.Write(TYPE_COMPLEX);
			var complexData = Serialize(value);
			writer.Write(complexData.Length);
			writer.Write(complexData);
		}

		// 字段值反序列化（用于同步系统）
		public static T DeserializeField<T>(BinaryReader reader)
		{
			byte typeId = reader.ReadByte();
			
			switch (typeId)
			{
				case TYPE_NULL:
					return default(T);
					
				case TYPE_PRIMITIVE:
					// 总是读取动态类型信息，因为序列化时总是写入了类型信息
					return (T)ReadDynamicPrimitiveValue(reader);
					
				case TYPE_COLLECTION_CHANGES:
					return (T)(object)CollectionUtils.DeserializeCollectionChanges(reader);
					
				case TYPE_SIMPLE_COLLECTION:
					return (T)DeserializeSimpleCollection(reader, typeof(T));
					
				case TYPE_COMPLEX:
					int length = reader.ReadInt32();
					byte[] data = reader.ReadBytes(length);
					return (T)Deserialize(typeof(T), data, 0, data.Length);
					
				default:
					throw new NotSupportedException($"不支持的类型标识符: {typeId}");
			}
		}

		#region 简单集合处理

		private static bool IsSimpleCollection(Type type)
		{
			if (!type.IsGenericType) return false;

			var genericTypeDef = type.GetGenericTypeDefinition();
			var genericArgs = type.GetGenericArguments();

			// 检查是否是支持的集合类型
			bool isSupportedCollection = genericTypeDef == typeof(List<>) ||
										 genericTypeDef == typeof(Dictionary<,>) ||
										 genericTypeDef == typeof(HashSet<>);

			if (!isSupportedCollection) return false;

			// 检查所有泛型参数是否都是基本类型
			foreach (var arg in genericArgs)
			{
				if (!IsPrimitiveType(arg)) return false;
			}

			return true;
		}

		private static void SerializeSimpleCollection(BinaryWriter writer, object collection)
		{
			var type = collection.GetType();
			var genericTypeDef = type.GetGenericTypeDefinition();

			if (genericTypeDef == typeof(List<>))
			{
				SerializeList(writer, collection);
			}
			else if (genericTypeDef == typeof(Dictionary<,>))
			{
				SerializeDictionary(writer, collection);
			}
			else if (genericTypeDef == typeof(HashSet<>))
			{
				SerializeHashSet(writer, collection);
			}
			else
			{
				throw new NotSupportedException($"不支持的简单集合类型: {type.Name}");
			}
		}

		private static object DeserializeSimpleCollection(BinaryReader reader, Type type)
		{
			var genericTypeDef = type.GetGenericTypeDefinition();

			if (genericTypeDef == typeof(List<>))
			{
				return DeserializeList(reader, type);
			}
			else if (genericTypeDef == typeof(Dictionary<,>))
			{
				return DeserializeDictionary(reader, type);
			}
			else if (genericTypeDef == typeof(HashSet<>))
			{
				return DeserializeHashSet(reader, type);
			}
			else
			{
				throw new NotSupportedException($"不支持的简单集合类型: {type.Name}");
			}
		}

		private static void SerializeList(BinaryWriter writer, object list)
		{
			var enumerable = (System.Collections.IEnumerable)list;
			var count = 0;
			var items = new List<object>();
			
			foreach (var item in enumerable)
			{
				items.Add(item);
				count++;
			}

			writer.Write(count);
			foreach (var item in items)
			{
				SerializeField(writer, item);
			}
		}

		private static object DeserializeList(BinaryReader reader, Type listType)
		{
			var elementType = listType.GetGenericArguments()[0];
			var list = Activator.CreateInstance(listType);
			var addMethod = listType.GetMethod("Add");
			
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				var item = typeof(TypeSerializers)
					.GetMethod(nameof(DeserializeField))
					.MakeGenericMethod(elementType)
					.Invoke(null, new object[] { reader });
				addMethod.Invoke(list, new[] { item });
			}

			return list;
		}

		private static void SerializeDictionary(BinaryWriter writer, object dict)
		{
			var enumerable = (System.Collections.IDictionary)dict;
			writer.Write(enumerable.Count);

			foreach (System.Collections.DictionaryEntry kvp in enumerable)
			{
				SerializeField(writer, kvp.Key);
				SerializeField(writer, kvp.Value);
			}
		}

		private static object DeserializeDictionary(BinaryReader reader, Type dictType)
		{
			var genericArgs = dictType.GetGenericArguments();
			var keyType = genericArgs[0];
			var valueType = genericArgs[1];
			var dict = Activator.CreateInstance(dictType);
			var addMethod = dictType.GetMethod("Add", new[] { keyType, valueType });

			int count = reader.ReadInt32();
			
			for (int i = 0; i < count; i++)
			{
				var key = typeof(TypeSerializers)
					.GetMethod(nameof(DeserializeField))
					.MakeGenericMethod(keyType)
					.Invoke(null, new object[] { reader });
					
				var value = typeof(TypeSerializers)
					.GetMethod(nameof(DeserializeField))
					.MakeGenericMethod(valueType)
					.Invoke(null, new object[] { reader });
					
				addMethod.Invoke(dict, new[] { key, value });
			}

			return dict;
		}

		private static void SerializeHashSet(BinaryWriter writer, object hashSet)
		{
			var enumerable = (System.Collections.IEnumerable)hashSet;
			var count = 0;
			var items = new List<object>();
			
			foreach (var item in enumerable)
			{
				items.Add(item);
				count++;
			}

			writer.Write(count);
			foreach (var item in items)
			{
				SerializeField(writer, item);
			}
		}

		private static object DeserializeHashSet(BinaryReader reader, Type hashSetType)
		{
			var elementType = hashSetType.GetGenericArguments()[0];
			var hashSet = Activator.CreateInstance(hashSetType);
			var addMethod = hashSetType.GetMethod("Add");
			
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				var item = typeof(TypeSerializers)
					.GetMethod(nameof(DeserializeField))
					.MakeGenericMethod(elementType)
					.Invoke(null, new object[] { reader });
				addMethod.Invoke(hashSet, new[] { item });
			}

			return hashSet;
		}

		#endregion

		#region 集合变更应用方法（委托给CollectionUtils）

		public static void ApplyCollectionChanges<T>(ICollection<T> collection, List<CollectionChange> changes)
		{
			CollectionUtils.ApplyCollectionChanges(collection, changes);
		}

		public static void ApplyDictionaryChanges<TKey, TValue>(IDictionary<TKey, TValue> dictionary, List<CollectionChange> changes)
		{
			CollectionUtils.ApplyDictionaryChanges(dictionary, changes);
		}

		public static void ApplyListChanges<T>(IList<T> list, List<CollectionChange> changes)
		{
			CollectionUtils.ApplyListChanges(list, changes);
		}

		#endregion

		#region 基本类型处理

		private static bool IsPrimitiveType(Type type)
		{
			return type == typeof(bool) ||
				   type == typeof(byte) || type == typeof(sbyte) ||
				   type == typeof(char) ||
				   type == typeof(decimal) ||
				   type == typeof(double) || type == typeof(float) ||
				   type == typeof(int) || type == typeof(uint) ||
				   type == typeof(long) || type == typeof(ulong) ||
				   type == typeof(short) || type == typeof(ushort) ||
				   type == typeof(string);
		}

		// 动态类型的基本值序列化（包含类型信息）
		private static void WriteDynamicPrimitiveValue(BinaryWriter writer, object value, Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean: 
					writer.Write(PRIMITIVE_BOOL);
					writer.Write((bool)value); 
					break;
				case TypeCode.Byte: 
					writer.Write(PRIMITIVE_BYTE);
					writer.Write((byte)value); 
					break;
				case TypeCode.SByte: 
					writer.Write(PRIMITIVE_SBYTE);
					writer.Write((sbyte)value); 
					break;
				case TypeCode.Char: 
					writer.Write(PRIMITIVE_CHAR);
					writer.Write((char)value); 
					break;
				case TypeCode.Decimal: 
					writer.Write(PRIMITIVE_DECIMAL);
					writer.Write((decimal)value); 
					break;
				case TypeCode.Double: 
					writer.Write(PRIMITIVE_DOUBLE);
					writer.Write((double)value); 
					break;
				case TypeCode.Single: 
					writer.Write(PRIMITIVE_FLOAT);
					writer.Write((float)value); 
					break;
				case TypeCode.Int32: 
					writer.Write(PRIMITIVE_INT);
					writer.Write((int)value); 
					break;
				case TypeCode.UInt32: 
					writer.Write(PRIMITIVE_UINT);
					writer.Write((uint)value); 
					break;
				case TypeCode.Int64: 
					writer.Write(PRIMITIVE_LONG);
					writer.Write((long)value); 
					break;
				case TypeCode.UInt64: 
					writer.Write(PRIMITIVE_ULONG);
					writer.Write((ulong)value); 
					break;
				case TypeCode.Int16: 
					writer.Write(PRIMITIVE_SHORT);
					writer.Write((short)value); 
					break;
				case TypeCode.UInt16: 
					writer.Write(PRIMITIVE_USHORT);
					writer.Write((ushort)value); 
					break;
				case TypeCode.String: 
					writer.Write(PRIMITIVE_STRING);
					writer.Write((string)value ?? string.Empty); 
					break;
				default: 
					throw new NotSupportedException($"不支持的基本类型: {type.Name}");
			}
		}

		// 动态类型的基本值反序列化（从类型信息读取）
		private static object ReadDynamicPrimitiveValue(BinaryReader reader)
		{
			byte primitiveType = reader.ReadByte();
			
			switch (primitiveType)
			{
				case PRIMITIVE_BOOL: return reader.ReadBoolean();
				case PRIMITIVE_BYTE: return reader.ReadByte();
				case PRIMITIVE_SBYTE: return reader.ReadSByte();
				case PRIMITIVE_CHAR: return reader.ReadChar();
				case PRIMITIVE_DECIMAL: return reader.ReadDecimal();
				case PRIMITIVE_DOUBLE: return reader.ReadDouble();
				case PRIMITIVE_FLOAT: return reader.ReadSingle();
				case PRIMITIVE_INT: return reader.ReadInt32();
				case PRIMITIVE_UINT: return reader.ReadUInt32();
				case PRIMITIVE_LONG: return reader.ReadInt64();
				case PRIMITIVE_ULONG: return reader.ReadUInt64();
				case PRIMITIVE_SHORT: return reader.ReadInt16();
				case PRIMITIVE_USHORT: return reader.ReadUInt16();
				case PRIMITIVE_STRING: return reader.ReadString();
				default: throw new NotSupportedException($"不支持的基本类型代码: {primitiveType}");
			}
		}

		// 静态类型的基本值序列化（不包含类型信息）
		private static void WritePrimitiveValue(BinaryWriter writer, object value, Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean: writer.Write((bool)value); break;
				case TypeCode.Byte: writer.Write((byte)value); break;
				case TypeCode.SByte: writer.Write((sbyte)value); break;
				case TypeCode.Char: writer.Write((char)value); break;
				case TypeCode.Decimal: writer.Write((decimal)value); break;
				case TypeCode.Double: writer.Write((double)value); break;
				case TypeCode.Single: writer.Write((float)value); break;
				case TypeCode.Int32: writer.Write((int)value); break;
				case TypeCode.UInt32: writer.Write((uint)value); break;
				case TypeCode.Int64: writer.Write((long)value); break;
				case TypeCode.UInt64: writer.Write((ulong)value); break;
				case TypeCode.Int16: writer.Write((short)value); break;
				case TypeCode.UInt16: writer.Write((ushort)value); break;
				case TypeCode.String: writer.Write((string)value ?? string.Empty); break;
				default: throw new NotSupportedException($"不支持的基本类型: {type.Name}");
			}
		}

		// 静态类型的基本值反序列化（已知类型）
		private static object ReadPrimitiveValue(BinaryReader reader, Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean: return reader.ReadBoolean();
				case TypeCode.Byte: return reader.ReadByte();
				case TypeCode.SByte: return reader.ReadSByte();
				case TypeCode.Char: return reader.ReadChar();
				case TypeCode.Decimal: return reader.ReadDecimal();
				case TypeCode.Double: return reader.ReadDouble();
				case TypeCode.Single: return reader.ReadSingle();
				case TypeCode.Int32: return reader.ReadInt32();
				case TypeCode.UInt32: return reader.ReadUInt32();
				case TypeCode.Int64: return reader.ReadInt64();
				case TypeCode.UInt64: return reader.ReadUInt64();
				case TypeCode.Int16: return reader.ReadInt16();
				case TypeCode.UInt16: return reader.ReadUInt16();
				case TypeCode.String: return reader.ReadString();
				default: throw new NotSupportedException($"不支持的基本类型: {type.Name}");
			}
		}

		#endregion
	}
} 
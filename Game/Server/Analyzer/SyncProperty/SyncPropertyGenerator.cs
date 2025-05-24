using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SyncCodeGen.SyncCodeGenUtils;

namespace SyncCodeGen
{
	public static class SyncPropertyGenerator
	{
		public static void GenerateFieldProperty(StringBuilder sb, string fieldName, string fieldType, string propertyName, string className)
		{
			sb.AppendLine();
			
			if (IsCollectionType(fieldType))
			{
				if (IsSyncCollectionType(fieldType))
				{
					// SyncCollection类型：使用CollectionChange机制
					sb.AppendLine($"{DoubleTab}public {fieldType} {propertyName} {{");
					sb.AppendLine($"{TripleTab}get => {fieldName};");
					sb.AppendLine($"{TripleTab}set {{");
					sb.AppendLine($"{TripleTab}{Tab}if (!ReferenceEquals({fieldName}, value)) {{");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}// SyncCollection替换，记录清空操作");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}AddCollectionChange(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(fieldName)}_Id, new SyncFramework.CollectionChange {{ Operation = SyncFramework.CollectionOperation.Clear }});");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}{fieldName} = value;");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}if ({fieldName} != null) {{");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}{Tab}(({fieldName} as SyncFramework.ISyncCollection))?.SetChangeCallback(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(fieldName)}_Id, AddCollectionChange);");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}}}");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}_dirtyState.Add(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(fieldName)}_Id);");
					sb.AppendLine($"{TripleTab}{Tab}}}");
					sb.AppendLine($"{TripleTab}}}");
					sb.AppendLine($"{DoubleTab}}}");
				}
				else
				{
					// 普通集合类型：直接整体更新
					sb.AppendLine($"{DoubleTab}public {fieldType} {propertyName} {{");
					sb.AppendLine($"{TripleTab}get => {fieldName};");
					sb.AppendLine($"{TripleTab}set {{");
					sb.AppendLine($"{TripleTab}{Tab}if (!ReferenceEquals({fieldName}, value)) {{");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}{fieldName} = value;");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}_dirtyState.Add(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(fieldName)}_Id);");
					sb.AppendLine($"{TripleTab}{Tab}}}");
					sb.AppendLine($"{TripleTab}}}");
					sb.AppendLine($"{DoubleTab}}}");
				}
			}
			else
			{
				// 普通属性
				sb.AppendLine($"{DoubleTab}public {fieldType} {propertyName} {{");
				sb.AppendLine($"{TripleTab}get => {fieldName};");
				sb.AppendLine($"{TripleTab}set {{");
				sb.AppendLine($"{TripleTab}{Tab}if(!EqualityComparer<{fieldType}>.Default.Equals({fieldName}, value)) {{");
				sb.AppendLine($"{TripleTab}{Tab}{Tab}{fieldName} = value;");
				sb.AppendLine($"{TripleTab}{Tab}{Tab}_dirtyState.Add(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(fieldName)}_Id);");
				sb.AppendLine($"{TripleTab}{Tab}}}");
				sb.AppendLine($"{TripleTab}}}");
				sb.AppendLine($"{DoubleTab}}}");
			}
		}

		public static void GenerateProperty(StringBuilder sb, string propName, string propType, string className)
		{
			sb.AppendLine();
			
			// 生成private字段来存储属性值
			string backingFieldName = GetBackingFieldName(propName);
			sb.AppendLine($"{DoubleTab}private {propType} {backingFieldName};");
			
			if (IsCollectionType(propType))
			{
				if (IsSyncCollectionType(propType))
				{
					// SyncCollection类型：使用CollectionChange机制
					sb.AppendLine($"{DoubleTab}public partial {propType} {propName} {{");
					sb.AppendLine($"{TripleTab}get => {backingFieldName};");
					sb.AppendLine($"{TripleTab}set {{");
					sb.AppendLine($"{TripleTab}{Tab}if (!ReferenceEquals({backingFieldName}, value)) {{");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}// SyncCollection替换，记录清空操作");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}AddCollectionChange(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(propName)}_Id, new SyncFramework.CollectionChange {{ Operation = SyncFramework.CollectionOperation.Clear }});");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}{backingFieldName} = value;");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}if ({backingFieldName} != null) {{");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}{Tab}(({backingFieldName} as SyncFramework.ISyncCollection))?.SetChangeCallback(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(propName)}_Id, AddCollectionChange);");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}}}");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}_dirtyState.Add(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(propName)}_Id);");
					sb.AppendLine($"{TripleTab}{Tab}}}");
					sb.AppendLine($"{TripleTab}}}");
					sb.AppendLine($"{DoubleTab}}}");
				}
				else
				{
					// 普通集合类型：直接整体更新
					sb.AppendLine($"{DoubleTab}public partial {propType} {propName} {{");
					sb.AppendLine($"{TripleTab}get => {backingFieldName};");
					sb.AppendLine($"{TripleTab}set {{");
					sb.AppendLine($"{TripleTab}{Tab}if (!ReferenceEquals({backingFieldName}, value)) {{");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}{backingFieldName} = value;");
					sb.AppendLine($"{TripleTab}{Tab}{Tab}_dirtyState.Add(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(propName)}_Id);");
					sb.AppendLine($"{TripleTab}{Tab}}}");
					sb.AppendLine($"{TripleTab}}}");
					sb.AppendLine($"{DoubleTab}}}");
				}
			}
			else
			{
				// 普通属性实现
				sb.AppendLine($"{DoubleTab}public partial {propType} {propName} {{");
				sb.AppendLine($"{TripleTab}get => {backingFieldName};");
				sb.AppendLine($"{TripleTab}set {{");
				sb.AppendLine($"{TripleTab}{Tab}if(!EqualityComparer<{propType}>.Default.Equals({backingFieldName}, value)) {{");
				sb.AppendLine($"{TripleTab}{Tab}{Tab}{backingFieldName} = value;");
				sb.AppendLine($"{TripleTab}{Tab}{Tab}_dirtyState.Add(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(propName)}_Id);");
				sb.AppendLine($"{TripleTab}{Tab}}}");
				sb.AppendLine($"{TripleTab}}}");
				sb.AppendLine($"{DoubleTab}}}");
			}
		}

		public static void GenerateSerializeField(StringBuilder sb, string fieldName, string fieldType, string currentClassName)
		{
			if (IsPrimitiveType(fieldType))
			{
				sb.AppendLine($"{QuintTab}writer.Write({fieldName});");
			}
			else if (IsCollectionType(fieldType) && IsSyncCollectionType(fieldType))
			{
				// SyncCollection类型：序列化变更记录
				string changesVar = $"changes_{fieldName}";
				sb.AppendLine($"{QuintTab}if (_collectionChanges.TryGetValue(SyncFramework.SyncMetadata.Ids.{currentClassName}_{Capitalize(fieldName)}_Id, out var {changesVar})) {{");
				sb.AppendLine($"{QuintTab}{Tab}SyncFramework.TypeSerializers.SerializeField(writer, {changesVar});");
				sb.AppendLine($"{QuintTab}}} else {{");
				sb.AppendLine($"{QuintTab}{Tab}SyncFramework.TypeSerializers.SerializeField(writer, new List<SyncFramework.CollectionChange>());");
				sb.AppendLine($"{QuintTab}}}");
			}
			else
			{
				// 普通类型和普通集合：直接序列化对象
				sb.AppendLine($"{QuintTab}SyncFramework.TypeSerializers.SerializeField(writer, {fieldName});");
			}
		}

		public static void GenerateSerializeProperty(StringBuilder sb, string propName, string propType, string currentClassName)
		{
			string backingFieldName = GetBackingFieldName(propName);
			
			if (IsPrimitiveType(propType))
			{
				sb.AppendLine($"{QuintTab}writer.Write({backingFieldName});");
			}
			else if (IsCollectionType(propType) && IsSyncCollectionType(propType))
			{
				// SyncCollection类型：序列化变更记录
				string changesVar = $"changes_{propName}";
				sb.AppendLine($"{QuintTab}if (_collectionChanges.TryGetValue(SyncFramework.SyncMetadata.Ids.{currentClassName}_{Capitalize(propName)}_Id, out var {changesVar})) {{");
				sb.AppendLine($"{QuintTab}{Tab}SyncFramework.TypeSerializers.SerializeField(writer, {changesVar});");
				sb.AppendLine($"{QuintTab}}} else {{");
				sb.AppendLine($"{QuintTab}{Tab}SyncFramework.TypeSerializers.SerializeField(writer, new List<SyncFramework.CollectionChange>());");
				sb.AppendLine($"{QuintTab}}}");
			}
			else
			{
				// 普通类型和普通集合：直接序列化对象
				sb.AppendLine($"{QuintTab}SyncFramework.TypeSerializers.SerializeField(writer, {backingFieldName});");
			}
		}

		public static void GenerateDeserializeField(StringBuilder sb, string fieldName, string fieldType)
		{
			if (IsPrimitiveType(fieldType))
			{
				string readerMethod = GetReaderMethod(fieldType);
				sb.AppendLine($"{QuintTab}{fieldName} = reader.{readerMethod}();");
			}
			else if (IsCollectionType(fieldType) && IsSyncCollectionType(fieldType))
			{
				// SyncCollection类型：反序列化并应用变更
				string changesVar = $"changes_{fieldName}";
				sb.AppendLine($"{QuintTab}var {changesVar} = SyncFramework.TypeSerializers.DeserializeField<List<SyncFramework.CollectionChange>>(reader);");
				sb.AppendLine($"{QuintTab}({fieldName} as SyncFramework.ISyncCollection)?.ApplyChanges({changesVar});");
			}
			else
			{
				// 普通类型和普通集合：直接反序列化对象
				sb.AppendLine($"{QuintTab}{fieldName} = SyncFramework.TypeSerializers.DeserializeField<{fieldType}>(reader);");
			}
		}

		public static void GenerateDeserializeProperty(StringBuilder sb, string propName, string propType, string className)
		{
			string backingFieldName = GetBackingFieldName(propName);
			
			if (IsPrimitiveType(propType))
			{
				string readerMethod = GetReaderMethod(propType);
				sb.AppendLine($"{QuintTab}{backingFieldName} = reader.{readerMethod}();");
			}
			else if (IsCollectionType(propType) && IsSyncCollectionType(propType))
			{
				// SyncCollection类型：初始化 -> 应用变更 -> 设置回调
				string changesVar = $"changes_{propName}";
				sb.AppendLine($"{QuintTab}var {changesVar} = SyncFramework.TypeSerializers.DeserializeField<List<SyncFramework.CollectionChange>>(reader);");
				sb.AppendLine($"{QuintTab}if ({backingFieldName} == null) {{");
				sb.AppendLine($"{QuintTab}{Tab}{backingFieldName} = new {propType}();");
				sb.AppendLine($"{QuintTab}}}");
				sb.AppendLine($"{QuintTab}({backingFieldName} as SyncFramework.ISyncCollection)?.ApplyChanges({changesVar});");
				sb.AppendLine($"{QuintTab}({backingFieldName} as SyncFramework.ISyncCollection)?.SetChangeCallback(SyncFramework.SyncMetadata.Ids.{className}_{Capitalize(propName)}_Id, AddCollectionChange);");
			}
			else
			{
				// 普通类型和普通集合：直接反序列化对象
				sb.AppendLine($"{QuintTab}{backingFieldName} = SyncFramework.TypeSerializers.DeserializeField<{propType}>(reader);");
			}
		}
	}
} 
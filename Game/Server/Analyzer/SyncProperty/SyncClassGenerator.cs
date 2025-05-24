using System.Linq;using System.Text;using static SyncCodeGen.SyncCodeGenUtils;

namespace SyncCodeGen
{
	public static class SyncClassGenerator
	{
		public static string GenerateSyncClass(SyncClassInfo classInfo)
		{
			var sb = new StringBuilder();

			sb.AppendLine("using System;");
			sb.AppendLine("using System.Collections.Generic;");
			sb.AppendLine("using System.IO;");
			sb.AppendLine("using System.Linq;");
			sb.AppendLine();
			sb.AppendLine($"namespace {classInfo.Namespace} {{");
			sb.AppendLine($"{Tab}public partial class {classInfo.ClassName} : SyncFramework.ISyncable<{classInfo.ClassName}> {{");

			// 类ID实现
			sb.AppendLine($"{DoubleTab}public int ClassId => SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_ClassId;");

			// 脏状态跟踪
			sb.AppendLine($"{DoubleTab}private HashSet<int> _dirtyState = new HashSet<int>();");
			sb.AppendLine($"{DoubleTab}public bool IsDirty => _dirtyState.Count > 0;");
			sb.AppendLine($"{DoubleTab}public int DirtyCount => _dirtyState.Count;");

			// 集合变更跟踪
			sb.AppendLine($"{DoubleTab}private Dictionary<int, List<SyncFramework.CollectionChange>> _collectionChanges = new Dictionary<int, List<SyncFramework.CollectionChange>>();");

			// 字段的属性
			foreach (var field in classInfo.SyncFields)
			{
				string propertyName = GetPropertyNameFromField(field.Name);
				SyncPropertyGenerator.GenerateFieldProperty(sb, field.Name, field.Type, propertyName, classInfo.ClassName);
			}

			// partial property实现
			foreach (var prop in classInfo.SyncProperties)
			{
				SyncPropertyGenerator.GenerateProperty(sb, prop.Name, prop.Type, classInfo.ClassName);
			}

			// 序列化方法
			GenerateSerializeMethods(sb, classInfo);

			// 反序列化方法
			GenerateDeserializeMethods(sb, classInfo);

			// 清除脏状态
			sb.AppendLine();
			sb.AppendLine($"{DoubleTab}public void ClearDirtyState() {{");
			sb.AppendLine($"{TripleTab}_dirtyState.Clear();");
			sb.AppendLine($"{TripleTab}_collectionChanges.Clear();");
			sb.AppendLine($"{DoubleTab}}}");

			// 集合变更帮助方法
			sb.AppendLine();
			sb.AppendLine($"{DoubleTab}// 集合变更帮助方法");
			sb.AppendLine($"{DoubleTab}private void AddCollectionChange(int fieldId, SyncFramework.CollectionChange change) {{");
			sb.AppendLine($"{TripleTab}if (!_collectionChanges.TryGetValue(fieldId, out var changes)) {{");
			sb.AppendLine($"{TripleTab}{Tab}changes = new List<SyncFramework.CollectionChange>();");
			sb.AppendLine($"{TripleTab}{Tab}_collectionChanges[fieldId] = changes;");
			sb.AppendLine($"{TripleTab}}}");
			sb.AppendLine($"{TripleTab}changes.Add(change);");
			sb.AppendLine($"{TripleTab}_dirtyState.Add(fieldId);");
			sb.AppendLine($"{DoubleTab}}}");

			// SyncCollection的辅助方法（只为SyncCollection类型生成）
			GenerateCollectionMethodsForClass(sb, classInfo);

			sb.AppendLine($"{Tab}}}");
			sb.AppendLine("}");

			return sb.ToString();
		}

		private static void GenerateCollectionMethodsForClass(StringBuilder sb, SyncClassInfo classInfo)
		{
			// 只为SyncCollection类型生成辅助方法
			foreach (var field in classInfo.SyncFields)
			{
				if (IsSyncCollectionType(field.Type))
				{
					GenerateFieldCollectionMethods(sb, field.Name, field.Type, classInfo.ClassName);
				}
			}

			foreach (var prop in classInfo.SyncProperties)
			{
				if (IsSyncCollectionType(prop.Type))
				{
					GeneratePropertyCollectionMethods(sb, prop.Name, prop.Type, classInfo.ClassName);
				}
			}
		}

		private static void GenerateFieldCollectionMethods(StringBuilder sb, string fieldName, string fieldType, string className)
		{
			sb.AppendLine();
			sb.AppendLine($"{DoubleTab}// {fieldName} SyncCollection辅助方法");

			if (fieldType.StartsWith("SyncList<") || fieldType.StartsWith("SyncFramework.SyncList<"))
			{
				string itemType = GetGenericTypeArguments(fieldType);
				string propertyName = GetPropertyNameFromField(fieldName);

				// Add方法
				sb.AppendLine($"{DoubleTab}public void Add{propertyName}({itemType} item)");
				sb.AppendLine($"{DoubleTab}{{");
				sb.AppendLine($"{TripleTab}if ({fieldName} == null) {fieldName} = new SyncFramework.SyncList<{itemType}>();");
				sb.AppendLine($"{TripleTab}{fieldName}.Add(item);");
				sb.AppendLine($"{DoubleTab}}}");

				// Remove方法
				sb.AppendLine($"{DoubleTab}public bool Remove{propertyName}({itemType} item)");
				sb.AppendLine($"{DoubleTab}{{");
				sb.AppendLine($"{TripleTab}return {fieldName}?.Remove(item) ?? false;");
				sb.AppendLine($"{DoubleTab}}}");

				// Clear方法
				sb.AppendLine($"{DoubleTab}public void Clear{propertyName}()");
				sb.AppendLine($"{DoubleTab}{{");
				sb.AppendLine($"{TripleTab}{fieldName}?.Clear();");
				sb.AppendLine($"{DoubleTab}}}");
			}
			else if (fieldType.StartsWith("SyncDictionary<") || fieldType.StartsWith("SyncFramework.SyncDictionary<"))
			{
				var genericArgs = GetGenericTypeArguments(fieldType);
				var types = genericArgs.Split(',').Select(t => t.Trim()).ToArray();
				if (types.Length == 2)
				{
					string keyType = types[0];
					string valueType = types[1];
					string propertyName = GetPropertyNameFromField(fieldName);

					// Set方法
					sb.AppendLine($"{DoubleTab}public void Set{propertyName}({keyType} key, {valueType} value)");
					sb.AppendLine($"{DoubleTab}{{");
					sb.AppendLine($"{TripleTab}if ({fieldName} == null) {fieldName} = new SyncFramework.SyncDictionary<{keyType}, {valueType}>();");
					sb.AppendLine($"{TripleTab}{fieldName}[key] = value;");
					sb.AppendLine($"{DoubleTab}}}");

					// Remove方法
					sb.AppendLine($"{DoubleTab}public bool Remove{propertyName}({keyType} key)");
					sb.AppendLine($"{DoubleTab}{{");
					sb.AppendLine($"{TripleTab}return {fieldName}?.Remove(key) ?? false;");
					sb.AppendLine($"{DoubleTab}}}");

					// Clear方法
					sb.AppendLine($"{DoubleTab}public void Clear{propertyName}()");
					sb.AppendLine($"{DoubleTab}{{");
					sb.AppendLine($"{TripleTab}{fieldName}?.Clear();");
					sb.AppendLine($"{DoubleTab}}}");
				}
			}
		}

		private static void GeneratePropertyCollectionMethods(StringBuilder sb, string propName, string propType, string className)
		{
			sb.AppendLine();
			sb.AppendLine($"{DoubleTab}// {propName} SyncCollection辅助方法");

			string backingFieldName = GetBackingFieldName(propName);

			if (propType.StartsWith("SyncList<") || propType.StartsWith("SyncFramework.SyncList<"))
			{
				string itemType = GetGenericTypeArguments(propType);

				// Add方法
				sb.AppendLine($"{DoubleTab}public void Add{propName}({itemType} item)");
				sb.AppendLine($"{DoubleTab}{{");
				sb.AppendLine($"{TripleTab}if ({backingFieldName} == null) {backingFieldName} = new SyncFramework.SyncList<{itemType}>();");
				sb.AppendLine($"{TripleTab}{backingFieldName}.Add(item);");
				sb.AppendLine($"{DoubleTab}}}");

				// Remove方法
				sb.AppendLine($"{DoubleTab}public bool Remove{propName}({itemType} item)");
				sb.AppendLine($"{DoubleTab}{{");
				sb.AppendLine($"{TripleTab}return {backingFieldName}?.Remove(item) ?? false;");
				sb.AppendLine($"{DoubleTab}}}");

				// Clear方法
				sb.AppendLine($"{DoubleTab}public void Clear{propName}()");
				sb.AppendLine($"{DoubleTab}{{");
				sb.AppendLine($"{TripleTab}{backingFieldName}?.Clear();");
				sb.AppendLine($"{DoubleTab}}}");
			}
			else if (propType.StartsWith("SyncDictionary<") || propType.StartsWith("SyncFramework.SyncDictionary<"))
			{
				var genericArgs = GetGenericTypeArguments(propType);
				var types = genericArgs.Split(',').Select(t => t.Trim()).ToArray();
				if (types.Length == 2)
				{
					string keyType = types[0];
					string valueType = types[1];

					// Set方法
					sb.AppendLine($"{DoubleTab}public void Set{propName}({keyType} key, {valueType} value)");
					sb.AppendLine($"{DoubleTab}{{");
					sb.AppendLine($"{TripleTab}if ({backingFieldName} == null) {backingFieldName} = new SyncFramework.SyncDictionary<{keyType}, {valueType}>();");
					sb.AppendLine($"{TripleTab}{backingFieldName}[key] = value;");
					sb.AppendLine($"{DoubleTab}}}");

					// Remove方法
					sb.AppendLine($"{DoubleTab}public bool Remove{propName}({keyType} key)");
					sb.AppendLine($"{DoubleTab}{{");
					sb.AppendLine($"{TripleTab}return {backingFieldName}?.Remove(key) ?? false;");
					sb.AppendLine($"{DoubleTab}}}");

					// Clear方法
					sb.AppendLine($"{DoubleTab}public void Clear{propName}()");
					sb.AppendLine($"{DoubleTab}{{");
					sb.AppendLine($"{TripleTab}{backingFieldName}?.Clear();");
					sb.AppendLine($"{DoubleTab}}}");
				}
			}
		}

		private static void GenerateSerializeMethods(StringBuilder sb, SyncClassInfo classInfo)
		{
			sb.AppendLine();
			sb.AppendLine($"{DoubleTab}public void SerializeDirtyValues(BinaryWriter writer) {{");
			sb.AppendLine($"{TripleTab}// 写入脏字段数量");
			sb.AppendLine($"{TripleTab}writer.Write(_dirtyState.Count);");
			sb.AppendLine();
			sb.AppendLine($"{TripleTab}// 写入每个脏字段");
			sb.AppendLine($"{TripleTab}foreach (int fieldId in _dirtyState) {{");
			sb.AppendLine($"{TripleTab}{Tab}writer.Write(fieldId);");
			sb.AppendLine($"{TripleTab}{Tab}switch (fieldId) {{");

			foreach (var field in classInfo.SyncFields)
			{
				sb.AppendLine($"{TripleTab}{Tab}{Tab}case SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_{Capitalize(field.Name)}_Id:");
				SyncPropertyGenerator.GenerateSerializeField(sb, field.Name, field.Type, classInfo.ClassName);
				sb.AppendLine($"{TripleTab}{Tab}{Tab}break;");
			}

			foreach (var prop in classInfo.SyncProperties)
			{
				sb.AppendLine($"{TripleTab}{Tab}{Tab}case SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_{Capitalize(prop.Name)}_Id:");
				SyncPropertyGenerator.GenerateSerializeProperty(sb, prop.Name, prop.Type, classInfo.ClassName);
				sb.AppendLine($"{TripleTab}{Tab}{Tab}break;");
			}

			sb.AppendLine($"{TripleTab}{Tab}}}");
			sb.AppendLine($"{TripleTab}}}");
			sb.AppendLine($"{DoubleTab}}}");
		}

		private static void GenerateDeserializeMethods(StringBuilder sb, SyncClassInfo classInfo)
		{
			sb.AppendLine();
			sb.AppendLine($"{DoubleTab}public void DeserializeValues(BinaryReader reader) {{");
			sb.AppendLine($"{TripleTab}int fieldCount = reader.ReadInt32();");
			sb.AppendLine();
			sb.AppendLine($"{TripleTab}for (int i = 0; i < fieldCount; i++) {{");
			sb.AppendLine($"{TripleTab}{Tab}int fieldId = reader.ReadInt32();");
			sb.AppendLine($"{TripleTab}{Tab}switch (fieldId) {{");

			foreach (var field in classInfo.SyncFields)
			{
				sb.AppendLine($"{TripleTab}{Tab}{Tab}case SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_{Capitalize(field.Name)}_Id:");
				SyncPropertyGenerator.GenerateDeserializeField(sb, field.Name, field.Type);
				sb.AppendLine($"{TripleTab}{Tab}{Tab}break;");
			}

			foreach (var prop in classInfo.SyncProperties)
			{
				sb.AppendLine($"{TripleTab}{Tab}{Tab}case SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_{Capitalize(prop.Name)}_Id:");
				SyncPropertyGenerator.GenerateDeserializeProperty(sb, prop.Name, prop.Type, classInfo.ClassName);
				sb.AppendLine($"{TripleTab}{Tab}{Tab}break;");
			}

			sb.AppendLine($"{TripleTab}{Tab}}}");
			sb.AppendLine($"{TripleTab}}}");
			sb.AppendLine($"{DoubleTab}}}");
		}
	}
} 
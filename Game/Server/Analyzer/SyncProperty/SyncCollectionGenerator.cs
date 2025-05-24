using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SyncCodeGen.SyncCodeGenUtils;

namespace SyncCodeGen
{
	public static class SyncCollectionGenerator
	{
		public static void GenerateCollectionMethods(StringBuilder sb, List<SyncClassInfo> syncClasses)
		{
			foreach (var classInfo in syncClasses)
			{
				GenerateClassCollectionMethods(sb, classInfo);
			}
		}

		private static void GenerateClassCollectionMethods(StringBuilder sb, SyncClassInfo classInfo)
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
	}
} 
using System.Collections.Generic;
using System.Text;
using static SyncCodeGen.SyncCodeGenUtils;

namespace SyncCodeGen
{
	public static class SyncMetadataGenerator
	{
		public static string GenerateMetadata(List<SyncClassInfo> syncClasses)
		{
			var sb = new StringBuilder();
			sb.AppendLine("using System;");
			sb.AppendLine("using System.Collections.Generic;");
			sb.AppendLine();
			sb.AppendLine("namespace SyncFramework {");
			sb.AppendLine($"{Tab}public static partial class SyncMetadata {{");

			// 类ID与名称映射
			sb.AppendLine($"{DoubleTab}// 类ID映射");
			sb.AppendLine($"{DoubleTab}public static readonly Dictionary<int, string> ClassNames = new Dictionary<int, string> {{");
			int nextClassId = 1;
			foreach (var classInfo in syncClasses)
			{
				sb.AppendLine($"{TripleTab}{{ {nextClassId}, \"{classInfo.Namespace}.{classInfo.ClassName}\" }},");
				nextClassId++;
			}
			sb.AppendLine($"{DoubleTab}}};");
			sb.AppendLine();

			// 字段ID常量
			sb.AppendLine($"{DoubleTab}// 字段ID常量");
			sb.AppendLine($"{DoubleTab}public static partial class Ids {{");
			nextClassId = 1;
			foreach (var classInfo in syncClasses)
			{
				sb.AppendLine($"{TripleTab}// {classInfo.ClassName}的ID和字段ID");
				sb.AppendLine($"{TripleTab}public const int {classInfo.ClassName}_ClassId = {nextClassId++};");

				int nextFieldId = 1;
				foreach (var field in classInfo.SyncFields)
				{
					sb.AppendLine($"{TripleTab}public const int {classInfo.ClassName}_{Capitalize(field.Name)}_Id = {nextFieldId++};");
				}

				foreach (var prop in classInfo.SyncProperties)
				{
					sb.AppendLine($"{TripleTab}public const int {classInfo.ClassName}_{Capitalize(prop.Name)}_Id = {nextFieldId++};");
				}
				sb.AppendLine();
			}
			sb.AppendLine($"{DoubleTab}}}");
			sb.AppendLine();

			// 字段元数据
			sb.AppendLine($"{DoubleTab}// 字段元数据");
			sb.AppendLine($"{DoubleTab}public static readonly Dictionary<int, FieldMetadata> Fields = new Dictionary<int, FieldMetadata> {{");
			nextClassId = 1;
			foreach (var classInfo in syncClasses)
			{
				int nextFieldId = 1;
				foreach (var field in classInfo.SyncFields)
				{
					string memberName = GetPropertyNameFromField(field.Name);
					string typeString = field.Type;
					sb.AppendLine($"{TripleTab}{{ Ids.{classInfo.ClassName}_{Capitalize(field.Name)}_Id, new FieldMetadata(Ids.{classInfo.ClassName}_ClassId, \"{memberName}\", typeof({typeString})) }},");
					nextFieldId++;
				}

				foreach (var prop in classInfo.SyncProperties)
				{
					string typeString = prop.Type;
					sb.AppendLine($"{TripleTab}{{ Ids.{classInfo.ClassName}_{Capitalize(prop.Name)}_Id, new FieldMetadata(Ids.{classInfo.ClassName}_ClassId, \"{prop.Name}\", typeof({typeString})) }},");
					nextFieldId++;
				}
				nextClassId++;
			}
			sb.AppendLine($"{DoubleTab}}};");

			// 元数据类
			sb.AppendLine();
			sb.AppendLine($"{DoubleTab}public class FieldMetadata {{");
			sb.AppendLine($"{TripleTab}public int ClassId {{ get; }}");
			sb.AppendLine($"{TripleTab}public string FieldName {{ get; }}");
			sb.AppendLine($"{TripleTab}public Type FieldType {{ get; }}");
			sb.AppendLine();
			sb.AppendLine($"{TripleTab}public FieldMetadata(int classId, string fieldName, Type fieldType) {{");
			sb.AppendLine($"{TripleTab}{Tab}ClassId = classId;");
			sb.AppendLine($"{TripleTab}{Tab}FieldName = fieldName;");
			sb.AppendLine($"{TripleTab}{Tab}FieldType = fieldType;");
			sb.AppendLine($"{TripleTab}}}");
			sb.AppendLine($"{DoubleTab}}}");

			sb.AppendLine($"{Tab}}}");
			sb.AppendLine("}");

			return sb.ToString();
		}
	}
} 
namespace SyncCodeGen
{
	public static class SyncCodeGenUtils
	{
		public const string Tab = "\t";
		public const string DoubleTab = "\t\t";
		public const string TripleTab = "\t\t\t";
		public const string QuadTab = "\t\t\t\t";
		public const string QuintTab = "\t\t\t\t\t";

		public static bool IsCollectionType(string typeName)
		{
			return typeName.StartsWith("List<") ||
				   typeName.StartsWith("IList<") ||
				   typeName.StartsWith("Dictionary<") ||
				   typeName.StartsWith("IDictionary<") ||
				   typeName.StartsWith("Queue<") ||
				   typeName.StartsWith("Stack<") ||
				   typeName.StartsWith("HashSet<") ||
				   typeName.StartsWith("SyncList<") ||
				   typeName.StartsWith("SyncDictionary<") ||
				   // 支持完整命名空间的类型名称
				   typeName.StartsWith("System.Collections.Generic.List<") ||
				   typeName.StartsWith("System.Collections.Generic.IList<") ||
				   typeName.StartsWith("System.Collections.Generic.Dictionary<") ||
				   typeName.StartsWith("System.Collections.Generic.IDictionary<") ||
				   typeName.StartsWith("System.Collections.Generic.Queue<") ||
				   typeName.StartsWith("System.Collections.Generic.Stack<") ||
				   typeName.StartsWith("System.Collections.Generic.HashSet<") ||
				   typeName.StartsWith("SyncFramework.SyncList<") ||
				   typeName.StartsWith("SyncFramework.SyncDictionary<") ||
				   typeName.EndsWith("[]");
		}

		public static bool IsSyncCollectionType(string typeName)
		{
			return typeName.StartsWith("SyncList<") ||
				   typeName.StartsWith("SyncDictionary<") ||
				   typeName.StartsWith("SyncFramework.SyncList<") ||
				   typeName.StartsWith("SyncFramework.SyncDictionary<");
		}

		public static bool IsPrimitiveType(string typeName)
		{
			return typeName switch
			{
				"bool" => true,
				"byte" => true,
				"sbyte" => true,
				"char" => true,
				"decimal" => true,
				"double" => true,
				"float" => true,
				"int" => true,
				"uint" => true,
				"long" => true,
				"ulong" => true,
				"short" => true,
				"ushort" => true,
				"string" => true,
				_ => false
			};
		}

		public static string GetReaderMethod(string typeName)
		{
			return typeName switch
			{
				"bool" => "ReadBoolean",
				"byte" => "ReadByte",
				"sbyte" => "ReadSByte",
				"char" => "ReadChar",
				"decimal" => "ReadDecimal",
				"double" => "ReadDouble",
				"float" => "ReadSingle",
				"int" => "ReadInt32",
				"uint" => "ReadUInt32",
				"long" => "ReadInt64",
				"ulong" => "ReadUInt64",
				"short" => "ReadInt16",
				"ushort" => "ReadUInt16",
				"string" => "ReadString",
				_ => "ReadObject"
			};
		}

		public static string GetPropertyNameFromField(string fieldName)
		{
			if (fieldName.StartsWith("_"))
			{
				return fieldName.Substring(1);
			}
			if (fieldName.StartsWith("m_"))
			{
				return fieldName.Substring(2);
			}
			return char.ToUpper(fieldName[0]) + fieldName.Substring(1);
		}

		public static string GetBackingFieldName(string propName)
		{
			return $"_{propName.Substring(0, 1).ToLower()}{propName.Substring(1)}";
		}

		public static string GetGenericTypeArguments(string typeName)
		{
			if (!typeName.Contains("<")) return string.Empty;
			
			var start = typeName.IndexOf('<') + 1;
			var end = typeName.LastIndexOf('>');
			if (end <= start) return string.Empty;
			
			return typeName.Substring(start, end - start);
		}

		public static string Capitalize(string str)
		{
			if (string.IsNullOrEmpty(str)) return str;
			return char.ToUpper(str[0]) + str.Substring(1);
		}
	}
} 
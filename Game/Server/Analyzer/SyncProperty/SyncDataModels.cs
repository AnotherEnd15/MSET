using System.Collections.Generic;

namespace SyncCodeGen
{
	public class SyncClassInfo
	{
		public string ClassName { get; set; } = string.Empty;
		public string Namespace { get; set; } = string.Empty;
		public List<SyncFieldInfo> SyncFields { get; set; } = new List<SyncFieldInfo>();
		public List<SyncPropertyInfo> SyncProperties { get; set; } = new List<SyncPropertyInfo>();
	}

	public class SyncFieldInfo
	{
		public string Name { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
	}

	public class SyncPropertyInfo
	{
		public string Name { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
	}
} 
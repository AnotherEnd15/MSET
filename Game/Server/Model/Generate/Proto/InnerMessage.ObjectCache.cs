using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[ResponseType(typeof(EntitySaveResponse))]
	[Message(10031)]
	[MemoryPackable]
	public partial class EntitySaveRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Zone { get; set; }

		[MemoryPackOrder(2)]
		public List<byte[]> Entitys { get; set; } = new (); 


		[MemoryPackOrder(3)]
		public List<string> CollectionNames { get; set; } = new (); 


		[MemoryPackOrder(4)]
		public bool SaveImd { get; set; }

		[MemoryPackOrder(5)]
		public long SendTime { get; set; }
	}

	[Message(10032)]
	[MemoryPackable]
	public partial class EntitySaveResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(QueryEntityResponse))]
	[Message(10033)]
	[MemoryPackable]
	public partial class QueryEntityRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Zone { get; set; }

		[MemoryPackOrder(2)]
		public List<string> TypeList { get; set; } = new (); 


		[MemoryPackOrder(3)]
		public List<long> IdList { get; set; } = new (); 


		[MemoryPackOrder(4)]
		public List<string> CollectionNames { get; set; } = new (); 

	}

	[Message(10034)]
	[MemoryPackable]
	public partial class QueryEntityResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public List<byte[]> EntityList { get; set; } = new (); 

	}

	[ResponseType(typeof(DeleteEntityResponse))]
	[Message(10035)]
	[MemoryPackable]
	public partial class DeleteEntityRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Zone { get; set; }

		[MemoryPackOrder(2)]
		public long Id { get; set; }

		[MemoryPackOrder(3)]
		public string Collection { get; set; }

		[MemoryPackOrder(4)]
		public string Type { get; set; }

		[MemoryPackOrder(5)]
		public bool DeleteFromDB { get; set; }
	}

	[Message(10036)]
	[MemoryPackable]
	public partial class DeleteEntityResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(QuerySavingCountResponse))]
	[Message(10037)]
	[MemoryPackable]
	public partial class QuerySavingCountRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(10038)]
	[MemoryPackable]
	public partial class QuerySavingCountResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public int Count { get; set; }
	}

public static partial class InnerMessage
	{
	 public const ushort EntitySaveRequest = 10031;
	 public const ushort EntitySaveResponse = 10032;
	 public const ushort QueryEntityRequest = 10033;
	 public const ushort QueryEntityResponse = 10034;
	 public const ushort DeleteEntityRequest = 10035;
	 public const ushort DeleteEntityResponse = 10036;
	 public const ushort QuerySavingCountRequest = 10037;
	 public const ushort QuerySavingCountResponse = 10038;
}

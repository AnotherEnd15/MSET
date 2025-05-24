using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[ResponseType(typeof(DP2Any_DBOperationResponse))]
	[Message(10001)]
	public partial class Any2DP_DBOperationRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

		public DBOperationRequest Request { get; set; }
	}

	[Message(10002)]
	public partial class DP2Any_DBOperationResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public BsonDocument Response { get; set; }
	}

	[ResponseType(typeof(DP2Any_QuerySavingCountResponse))]
	[Message(10003)]
	public partial class Any2DP_QuerySavingCountRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

	}

	[Message(10004)]
	public partial class DP2Any_QuerySavingCountResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public int Count { get; set; }
	}

public static partial class InnerMessage
	{
	 public const ushort Any2DP_DBOperationRequest = 10001;
	 public const ushort DP2Any_DBOperationResponse = 10002;
	 public const ushort Any2DP_QuerySavingCountRequest = 10003;
	 public const ushort DP2Any_QuerySavingCountResponse = 10004;
}

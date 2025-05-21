using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[ResponseType(typeof(Any2Gate_QueryOnlineAccountsResponse))]
	[Message(10004)]
	[MemoryPackable]
	public partial class Any2Gate_QueryOnlineAccountsRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(10005)]
	[MemoryPackable]
	public partial class Any2Gate_QueryOnlineAccountsResponse : IActorResponse
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

	[Message(10006)]
	[MemoryPackable]
	public partial class GM2Gate_KickOutPlayer : IActorMessage
	{
		[MemoryPackOrder(0)]
		public string OpenId { get; set; }
	}

	[MemoryPackable]
	public partial class PlayerLoginInfo
	{
		[MemoryPackOrder(0)]
		public string OpenId { get; set; }

		[MemoryPackOrder(1)]
		public string SessionKey { get; set; }

		[MemoryPackOrder(2)]
		public string Code { get; set; }
	}

	[Message(10007)]
	[MemoryPackable]
	public partial class Game2Gate_CloseSession : IActorMessage
	{
	}

public static partial class InnerMessage
	{
	 public const ushort Any2Gate_QueryOnlineAccountsRequest = 10004;
	 public const ushort Any2Gate_QueryOnlineAccountsResponse = 10005;
	 public const ushort GM2Gate_KickOutPlayer = 10006;
	 public const ushort Game2Gate_CloseSession = 10007;
}

using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[ResponseType(typeof(Any2Gate_QueryOnlineAccountsResponse))]
	[Message(10008)]
	public partial class Any2Gate_QueryOnlineAccountsRequest : IActorRequest
	{
		public int RpcId { get; set; }

	}

	[Message(10009)]
	public partial class Any2Gate_QueryOnlineAccountsResponse : IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public int Count { get; set; }
	}

	[Message(10010)]
	public partial class GM2Gate_KickOutPlayer : IActorMessage
	{
		public string OpenId { get; set; }
	}

	public partial class PlayerLoginInfo
	{
		public string OpenId { get; set; }

		public string SessionKey { get; set; }

		public string Code { get; set; }
	}

	[Message(10011)]
	public partial class Game2Gate_CloseSession : IActorMessage
	{
	}

public static partial class InnerMessage
	{
	 public const ushort Any2Gate_QueryOnlineAccountsRequest = 10008;
	 public const ushort Any2Gate_QueryOnlineAccountsResponse = 10009;
	 public const ushort GM2Gate_KickOutPlayer = 10010;
	 public const ushort Game2Gate_CloseSession = 10011;
}

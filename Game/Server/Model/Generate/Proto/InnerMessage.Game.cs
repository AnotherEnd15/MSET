using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[ResponseType(typeof(Game2G_EnterGameResponse))]
	[Message(10005)]
	public partial class G2Game_EnterGameRequest : IActorRequest
	{
		public int RpcId { get; set; }

		public int Uid { get; set; }

		public long GateActorId { get; set; }

		public long ClientSessionId { get; set; }
	}

	[Message(10006)]
	public partial class Game2G_EnterGameResponse : IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long PlayerActorId { get; set; }
	}

	[Message(10007)]
	public partial class G2Game_PlayerSessionClose : IActorMessage
	{
		public long SessionId { get; set; }
	}

public static partial class InnerMessage
	{
	 public const ushort G2Game_EnterGameRequest = 10005;
	 public const ushort Game2G_EnterGameResponse = 10006;
	 public const ushort G2Game_PlayerSessionClose = 10007;
}

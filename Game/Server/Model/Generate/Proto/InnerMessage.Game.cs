using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[ResponseType(typeof(Game2G_EnterGameResponse))]
	[Message(10001)]
	[MemoryPackable]
	public partial class G2Game_EnterGameRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Uid { get; set; }

		[MemoryPackOrder(2)]
		public long GateActorId { get; set; }

		[MemoryPackOrder(3)]
		public long ClientSessionId { get; set; }
	}

	[Message(10002)]
	[MemoryPackable]
	public partial class Game2G_EnterGameResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long PlayerActorId { get; set; }
	}

	[Message(10003)]
	[MemoryPackable]
	public partial class G2Game_PlayerSessionClose : IActorMessage
	{
		[MemoryPackOrder(0)]
		public long SessionId { get; set; }
	}

public static partial class InnerMessage
	{
	 public const ushort G2Game_EnterGameRequest = 10001;
	 public const ushort Game2G_EnterGameResponse = 10002;
	 public const ushort G2Game_PlayerSessionClose = 10003;
}

using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[ResponseType(typeof(Game2C_GMCommandResponse))]
	[Message(5)]
	[MemoryPackable]
	public partial class C2Game_GMCommandRequest : IActorPlayerRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public string Command { get; set; }

		[MemoryPackOrder(2)]
		public List<string> ParamList { get; set; } = new (); 

	}

	[Message(6)]
	[MemoryPackable]
	public partial class Game2C_GMCommandResponse : IActorPlayerResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(Game2C_GetGMCommandDefinesResponse))]
	[Message(7)]
	[MemoryPackable]
	public partial class C2Game_GetGMCommandDefinesRequest : IActorPlayerRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(8)]
	[MemoryPackable]
	public partial class Game2C_GetGMCommandDefinesResponse : IActorPlayerResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
		[MemoryPackOrder(3)]
		public Dictionary<string, string> Command2Defines { get; set; } = new (); 

	}

	[ResponseType(typeof(Game2C_ReloadConfigResponse))]
	[Message(9)]
	[MemoryPackable]
	public partial class C2Game_ReloadConfigRequest : IActorPlayerRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(10)]
	[MemoryPackable]
	public partial class Game2C_ReloadConfigResponse : IActorPlayerResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(Game2C_LogoutResponse))]
	[Message(11)]
	[MemoryPackable]
	public partial class C2Game_LogoutRequest : IActorPlayerRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(12)]
	[MemoryPackable]
	public partial class Game2C_LogoutResponse : IActorPlayerResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

public static partial class OuterMessage
	{
	 public const ushort C2Game_GMCommandRequest = 5;
	 public const ushort Game2C_GMCommandResponse = 6;
	 public const ushort C2Game_GetGMCommandDefinesRequest = 7;
	 public const ushort Game2C_GetGMCommandDefinesResponse = 8;
	 public const ushort C2Game_ReloadConfigRequest = 9;
	 public const ushort Game2C_ReloadConfigResponse = 10;
	 public const ushort C2Game_LogoutRequest = 11;
	 public const ushort Game2C_LogoutResponse = 12;
}

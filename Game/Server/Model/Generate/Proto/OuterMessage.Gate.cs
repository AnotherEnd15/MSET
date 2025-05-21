using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[MemoryPackable]
	public partial class HttpGetRouterResponse
	{
		[MemoryPackOrder(0)]
		public List<string> Realms { get; set; } = new (); 


		[MemoryPackOrder(1)]
		public List<string> Routers { get; set; } = new (); 

	}

	[MemoryPackable]
	public partial class RouterSync
	{
		[MemoryPackOrder(0)]
		public uint ConnectId { get; set; }

		[MemoryPackOrder(1)]
		public string Address { get; set; }
	}

	[ResponseType(typeof(G2C_LoginResponse))]
	[Message(13)]
	[MemoryPackable]
	public partial class C2G_LoginRequest : IRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public string Account { get; set; }

		[MemoryPackOrder(2)]
		public int GameVersion { get; set; }
	}

	[Message(14)]
	[MemoryPackable]
	public partial class G2C_LoginResponse : IResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(G2C_EnterGame))]
	[Message(15)]
	[MemoryPackable]
	public partial class C2G_EnterGame : IRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long SceneInstanceId { get; set; }
	}

	[Message(16)]
	[MemoryPackable]
	public partial class G2C_EnterGame : IResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long MyId { get; set; }
	}

	[ResponseType(typeof(G2C_ReconnectResponse))]
	[Message(17)]
	[MemoryPackable]
	public partial class C2G_ReconnectRequest : IRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public string Code { get; set; }

		[MemoryPackOrder(2)]
		public string OpenId { get; set; }
	}

	[Message(18)]
	[MemoryPackable]
	public partial class G2C_ReconnectResponse : IResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(G2C_Ping))]
	[Message(19)]
	[MemoryPackable]
	public partial class C2G_Ping : IRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(20)]
	[MemoryPackable]
	public partial class G2C_Ping : IResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long Time { get; set; }
	}

	[MemoryPackable]
	public partial class GatePlayerInfo
	{
		[MemoryPackOrder(0)]
		public int Zone { get; set; }

		[MemoryPackOrder(1)]
		public long UnitId { get; set; }

		[MemoryPackOrder(2)]
		public int Level { get; set; }

		[MemoryPackOrder(3)]
		public string Name { get; set; }

		[MemoryPackOrder(4)]
		public long LastLoginTime { get; set; }

		[MemoryPackOrder(5)]
		public string ProfilePic { get; set; }

		[MemoryPackOrder(6)]
		public int ProfileFrame { get; set; }
	}

	[ResponseType(typeof(G2C_Reload))]
	[Message(21)]
	[MemoryPackable]
	public partial class C2G_Reload : IRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public string Account { get; set; }

		[MemoryPackOrder(2)]
		public string Password { get; set; }
	}

	[Message(22)]
	[MemoryPackable]
	public partial class G2C_Reload : IResponse
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
	 public const ushort C2G_LoginRequest = 13;
	 public const ushort G2C_LoginResponse = 14;
	 public const ushort C2G_EnterGame = 15;
	 public const ushort G2C_EnterGame = 16;
	 public const ushort C2G_ReconnectRequest = 17;
	 public const ushort G2C_ReconnectResponse = 18;
	 public const ushort C2G_Ping = 19;
	 public const ushort G2C_Ping = 20;
	 public const ushort C2G_Reload = 21;
	 public const ushort G2C_Reload = 22;
}

using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[ResponseType(typeof(ObjectQueryResponse))]
	[Message(10008)]
	[MemoryPackable]
	public partial class ObjectQueryRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Key { get; set; }

		[MemoryPackOrder(2)]
		public long InstanceId { get; set; }
	}

	[Message(10009)]
	[MemoryPackable]
	public partial class ObjectQueryResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public byte[] Entity { get; set; }
	}

	[ResponseType(typeof(A2M_Reload))]
	[Message(10010)]
	[MemoryPackable]
	public partial class M2A_Reload : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(10011)]
	[MemoryPackable]
	public partial class A2M_Reload : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(G2G_LockResponse))]
	[Message(10012)]
	[MemoryPackable]
	public partial class G2G_LockRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(10013)]
	[MemoryPackable]
	public partial class G2G_LockResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(G2G_LockReleaseResponse))]
	[Message(10014)]
	[MemoryPackable]
	public partial class G2G_LockReleaseRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Id { get; set; }

		[MemoryPackOrder(2)]
		public string Address { get; set; }
	}

	[Message(10015)]
	[MemoryPackable]
	public partial class G2G_LockReleaseResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(ObjectAddResponse))]
	[Message(10016)]
	[MemoryPackable]
	public partial class ObjectAddRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Key { get; set; }

		[MemoryPackOrder(2)]
		public long InstanceId { get; set; }
	}

	[Message(10017)]
	[MemoryPackable]
	public partial class ObjectAddResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(ObjectLockResponse))]
	[Message(10018)]
	[MemoryPackable]
	public partial class ObjectLockRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Key { get; set; }

		[MemoryPackOrder(2)]
		public long InstanceId { get; set; }

		[MemoryPackOrder(3)]
		public int Time { get; set; }
	}

	[Message(10019)]
	[MemoryPackable]
	public partial class ObjectLockResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(ObjectUnLockResponse))]
	[Message(10020)]
	[MemoryPackable]
	public partial class ObjectUnLockRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Key { get; set; }

		[MemoryPackOrder(2)]
		public long OldInstanceId { get; set; }

		[MemoryPackOrder(3)]
		public long InstanceId { get; set; }
	}

	[Message(10021)]
	[MemoryPackable]
	public partial class ObjectUnLockResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(ObjectRemoveResponse))]
	[Message(10022)]
	[MemoryPackable]
	public partial class ObjectRemoveRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Key { get; set; }
	}

	[Message(10023)]
	[MemoryPackable]
	public partial class ObjectRemoveResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(typeof(ObjectGetResponse))]
	[Message(10024)]
	[MemoryPackable]
	public partial class ObjectGetRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Key { get; set; }
	}

	[Message(10025)]
	[MemoryPackable]
	public partial class ObjectGetResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long InstanceId { get; set; }
	}

	[ResponseType(typeof(BM2Game_AllocSceneResponse))]
	[Message(10026)]
	[MemoryPackable]
	public partial class Game2BM_AllocSceneRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long UnitId { get; set; }
	}

	[Message(10027)]
	[MemoryPackable]
	public partial class BM2Game_AllocSceneResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long SceneInstanceId { get; set; }
	}

	[ResponseType(typeof(B2BM_AllocSceneResponse))]
	[Message(10028)]
	[MemoryPackable]
	public partial class BM2B_AllocSceneRequest : IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long MasterPlayerId { get; set; }
	}

	[Message(10029)]
	[MemoryPackable]
	public partial class B2BM_AllocSceneResponse : IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long SceneInstanceId { get; set; }
	}

	[Message(10030)]
	[MemoryPackable]
	public partial class B2BM_DungeonDispose : IActorMessage
	{
		[MemoryPackOrder(0)]
		public uint MapId { get; set; }

		[MemoryPackOrder(1)]
		public long SceneInstanceId { get; set; }
	}

public static partial class InnerMessage
	{
	 public const ushort ObjectQueryRequest = 10008;
	 public const ushort ObjectQueryResponse = 10009;
	 public const ushort M2A_Reload = 10010;
	 public const ushort A2M_Reload = 10011;
	 public const ushort G2G_LockRequest = 10012;
	 public const ushort G2G_LockResponse = 10013;
	 public const ushort G2G_LockReleaseRequest = 10014;
	 public const ushort G2G_LockReleaseResponse = 10015;
	 public const ushort ObjectAddRequest = 10016;
	 public const ushort ObjectAddResponse = 10017;
	 public const ushort ObjectLockRequest = 10018;
	 public const ushort ObjectLockResponse = 10019;
	 public const ushort ObjectUnLockRequest = 10020;
	 public const ushort ObjectUnLockResponse = 10021;
	 public const ushort ObjectRemoveRequest = 10022;
	 public const ushort ObjectRemoveResponse = 10023;
	 public const ushort ObjectGetRequest = 10024;
	 public const ushort ObjectGetResponse = 10025;
	 public const ushort Game2BM_AllocSceneRequest = 10026;
	 public const ushort BM2Game_AllocSceneResponse = 10027;
	 public const ushort BM2B_AllocSceneRequest = 10028;
	 public const ushort B2BM_AllocSceneResponse = 10029;
	 public const ushort B2BM_DungeonDispose = 10030;
}

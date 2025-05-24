using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[ResponseType(typeof(ObjectQueryResponse))]
	[Message(10012)]
	public partial class ObjectQueryRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }

		public long InstanceId { get; set; }
	}

	[Message(10013)]
	public partial class ObjectQueryResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public byte[] Entity { get; set; }
	}

	[ResponseType(typeof(A2M_Reload))]
	[Message(10014)]
	public partial class M2A_Reload : Object, IActorRequest
	{
		public int RpcId { get; set; }

	}

	[Message(10015)]
	public partial class A2M_Reload : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[ResponseType(typeof(G2G_LockResponse))]
	[Message(10016)]
	public partial class G2G_LockRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

	}

	[Message(10017)]
	public partial class G2G_LockResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[ResponseType(typeof(G2G_LockReleaseResponse))]
	[Message(10018)]
	public partial class G2G_LockReleaseRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

		public long Id { get; set; }

		public string Address { get; set; }
	}

	[Message(10019)]
	public partial class G2G_LockReleaseResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[ResponseType(typeof(ObjectAddResponse))]
	[Message(10020)]
	public partial class ObjectAddRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }

		public long InstanceId { get; set; }
	}

	[Message(10021)]
	public partial class ObjectAddResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[ResponseType(typeof(ObjectLockResponse))]
	[Message(10022)]
	public partial class ObjectLockRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }

		public long InstanceId { get; set; }

		public int Time { get; set; }
	}

	[Message(10023)]
	public partial class ObjectLockResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[ResponseType(typeof(ObjectUnLockResponse))]
	[Message(10024)]
	public partial class ObjectUnLockRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }

		public long OldInstanceId { get; set; }

		public long InstanceId { get; set; }
	}

	[Message(10025)]
	public partial class ObjectUnLockResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[ResponseType(typeof(ObjectRemoveResponse))]
	[Message(10026)]
	public partial class ObjectRemoveRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }
	}

	[Message(10027)]
	public partial class ObjectRemoveResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[ResponseType(typeof(ObjectGetResponse))]
	[Message(10028)]
	public partial class ObjectGetRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }
	}

	[Message(10029)]
	public partial class ObjectGetResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long InstanceId { get; set; }
	}

	[ResponseType(typeof(BM2Game_AllocSceneResponse))]
	[Message(10030)]
	public partial class Game2BM_AllocSceneRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

		public long UnitId { get; set; }
	}

	[Message(10031)]
	public partial class BM2Game_AllocSceneResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long SceneInstanceId { get; set; }
	}

	[ResponseType(typeof(B2BM_AllocSceneResponse))]
	[Message(10032)]
	public partial class BM2B_AllocSceneRequest : Object, IActorRequest
	{
		public int RpcId { get; set; }

		public long MasterPlayerId { get; set; }
	}

	[Message(10033)]
	public partial class B2BM_AllocSceneResponse : Object, IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long SceneInstanceId { get; set; }
	}

	[Message(10034)]
	public partial class B2BM_DungeonDispose : Object, IActorMessage
	{
		public uint MapId { get; set; }

		public long SceneInstanceId { get; set; }
	}

public static partial class InnerMessage
	{
	 public const ushort ObjectQueryRequest = 10012;
	 public const ushort ObjectQueryResponse = 10013;
	 public const ushort M2A_Reload = 10014;
	 public const ushort A2M_Reload = 10015;
	 public const ushort G2G_LockRequest = 10016;
	 public const ushort G2G_LockResponse = 10017;
	 public const ushort G2G_LockReleaseRequest = 10018;
	 public const ushort G2G_LockReleaseResponse = 10019;
	 public const ushort ObjectAddRequest = 10020;
	 public const ushort ObjectAddResponse = 10021;
	 public const ushort ObjectLockRequest = 10022;
	 public const ushort ObjectLockResponse = 10023;
	 public const ushort ObjectUnLockRequest = 10024;
	 public const ushort ObjectUnLockResponse = 10025;
	 public const ushort ObjectRemoveRequest = 10026;
	 public const ushort ObjectRemoveResponse = 10027;
	 public const ushort ObjectGetRequest = 10028;
	 public const ushort ObjectGetResponse = 10029;
	 public const ushort Game2BM_AllocSceneRequest = 10030;
	 public const ushort BM2Game_AllocSceneResponse = 10031;
	 public const ushort BM2B_AllocSceneRequest = 10032;
	 public const ushort B2BM_AllocSceneResponse = 10033;
	 public const ushort B2BM_DungeonDispose = 10034;
}

using ET;
using System;
using MemoryPack;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET.Proto;
// ReSharper disable InconsistentNaming
	[Message(2)]
	[MemoryPackable]
	public partial class G2C_CloseServer : IMessage
	{
		[MemoryPackOrder(0)]
		public long CloseTime { get; set; }
	}

	[Message(3)]
	[MemoryPackable]
	public partial class G2C_RepeatLogin : IMessage
	{
	}

	[Message(4)]
	[MemoryPackable]
	public partial class G2C_EnterGameResult : IMessage
	{
		[MemoryPackOrder(0)]
		public int Error { get; set; }
	}

public static partial class OuterMessage
	{
	 public const ushort G2C_CloseServer = 2;
	 public const ushort G2C_RepeatLogin = 3;
	 public const ushort G2C_EnterGameResult = 4;
}

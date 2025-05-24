using System;
using System.IO;

namespace ET
{
	/// <summary>
	/// 消息序列化辅助类，负责网络消息的序列化和反序列化
	/// 支持内网(带ActorId)和外网(不带ActorId)两种消息格式
	/// 使用对象池化的MemoryStream以优化内存使用
	/// 消息格式：[ActorId(8字节,仅内网)][Opcode(2字节)][MessageBody]
	/// </summary>
	public static class MessageSerializeHelper
	{
		private static MemoryStream GetStream(int count = 0)
		{
			return MemoryStreamPool.Instance.Fetch(count);
		}

		public static (ushort, MemoryStream) MessageToStream(object message)
		{
			int headOffset = Packet.ActorIdLength;
			// 不指定大小，使用对象池的默认缓存大小(1024字节)
			MemoryStream stream = GetStream();

			ushort opcode = NetServices.Instance.GetOpcode(message.GetType());

			stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
			stream.SetLength(headOffset + Packet.OpcodeLength);

			// 使用Span<T>高效写入opcode，避免装箱和临时数组分配
			var buffer = stream.GetBuffer().AsSpan();
			BitConverter.TryWriteBytes(buffer.Slice(headOffset, 2), opcode);
			
			SerializeHelper.Serialize(opcode, message, stream);
			
			stream.Seek(0, SeekOrigin.Begin);
			return (opcode, stream);
		}
	}
}
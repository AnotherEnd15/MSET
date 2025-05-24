using System;
using System.Buffers;
using System.IO;

namespace ET
{
	public enum ParserState
	{
		PacketSize,
		PacketBody
	}

	/// <summary>
	/// 网络数据包解析器，支持TCP和KCP流模式的长度前缀协议
	/// 从循环缓冲区中解析完整的数据包：[Length][Data]格式
	/// 内网使用4字节长度前缀，外网使用2字节，KCP流模式统一使用4字节
	/// </summary>
	public class PacketParser
	{
		private readonly CircularBuffer buffer;
		private int packetSize;
		private ParserState state;
		public AService service;
		private readonly byte[] cache = ArrayPool<byte>.Shared.Rent(8);
		public const int InnerPacketSizeLength = 4;
		public const int OuterPacketSizeLength = 2;
		/// <summary>
		/// KCP流模式统一使用4字节长度前缀，支持最大约4GB的消息
		/// </summary>
		public const int KcpPacketSizeLength = 4;
		public MemoryStream MemoryStream;

		public PacketParser(CircularBuffer buffer, AService service)
		{
			this.buffer = buffer;
			this.service = service;
		}

		~PacketParser()
		{
			// 确保析构时归还租用的数组，防止内存泄漏
			if (this.cache != null)
			{
				ArrayPool<byte>.Shared.Return(this.cache);
			}
		}

		public bool Parse()
		{
			while (true)
			{
				switch (this.state)
				{
					case ParserState.PacketSize:
					{
						// KCP流模式：统一使用4字节长度前缀，消除消息大小限制
						if (this.service is KService)
						{
							if (this.buffer.Length < KcpPacketSizeLength)
							{
								return false;
							}

							this.buffer.ReadExactly(this.cache, 0, KcpPacketSizeLength);
							this.packetSize = BitConverter.ToInt32(this.cache.AsSpan(0, KcpPacketSizeLength));
							
							if (this.packetSize > ushort.MaxValue * 64 || this.packetSize < 0)
							{
								throw new Exception($"KCP recv packet size error: {this.packetSize}");
							}
						}
						// TCP内网模式：使用4字节长度前缀
						else if (this.service.ServiceType == ServiceType.Inner)
						{
							if (this.buffer.Length < InnerPacketSizeLength)
							{
								return false;
							}

							this.buffer.ReadExactly(this.cache, 0, InnerPacketSizeLength);
							this.packetSize = BitConverter.ToInt32(this.cache.AsSpan(0, InnerPacketSizeLength));
							
							if (this.packetSize > ushort.MaxValue * 16 || this.packetSize < Packet.MinPacketSize)
							{
								throw new Exception($"recv packet size error, 可能是外网探测端口: {this.packetSize}");
							}
						}
						// TCP外网模式：使用2字节长度前缀
						else
						{
							if (this.buffer.Length < OuterPacketSizeLength)
							{
								return false;
							}

							this.buffer.ReadExactly(this.cache, 0, OuterPacketSizeLength);
							this.packetSize = BitConverter.ToUInt16(this.cache.AsSpan(0, OuterPacketSizeLength));
							
							if (this.packetSize < Packet.MinPacketSize)
							{
								throw new Exception($"recv packet size error, 可能是外网探测端口: {this.packetSize}");
							}
						}

						this.state = ParserState.PacketBody;
						break;
					}
					case ParserState.PacketBody:
					{
						if (this.buffer.Length < this.packetSize)
						{
							return false;
						}

						// 从对象池获取MemoryStream，避免频繁的内存分配
						MemoryStream memoryStream = MemoryStreamPool.Instance.Fetch(this.packetSize);
						memoryStream.SetLength(this.packetSize);
						memoryStream.Seek(0, SeekOrigin.Begin);
						
						this.buffer.Read(memoryStream, this.packetSize);
						this.MemoryStream = memoryStream;

						// 根据协议类型设置消息读取偏移位置
						if (this.service is KService)
						{
							// KCP流模式：根据服务类型设置不同的消息起始位置
							if (this.service.ServiceType == ServiceType.Inner)
							{
								memoryStream.Seek(Packet.ActorIdLength + Packet.OpcodeLength, SeekOrigin.Begin);
							}
							else
							{
								memoryStream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
							}
						}
						else
						{
							// TCP模式：使用标准的消息偏移量
							if (this.service.ServiceType == ServiceType.Inner)
							{
								memoryStream.Seek(Packet.MessageIndex, SeekOrigin.Begin);
							}
							else
							{
								memoryStream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
							}
						}

						this.state = ParserState.PacketSize;
						return true;
					}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}
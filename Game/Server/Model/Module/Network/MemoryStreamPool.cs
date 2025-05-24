#if UNITY_EDITOR || DOTNET
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;

namespace ET
{
    /// <summary>
    /// 内存流和字节数组对象池，用于减少GC压力
    /// 提供MemoryStream对象池和ArrayPool<byte>的封装
    /// 支持线程安全的内存复用
    /// </summary>
    public class MemoryStreamPool
    {
        [StaticField]
        public static MemoryStreamPool Instance = new();
        
        private const int MaxMemoryBufferSize = 1024;
        private const int MaxPoolSize = 32;

        private readonly ConcurrentQueue<MemoryStream> pool = new();
        
        /// <summary>
        /// 内部使用ArrayPool复用字节数组，避免频繁的内存分配
        /// </summary>
        private static readonly ArrayPool<byte> ByteArrayPool = ArrayPool<byte>.Shared;

        public MemoryStream Fetch(int size = 0)
        {
            if (size > MaxMemoryBufferSize)
            {
                return new MemoryStream(size);
            }

            if (size < MaxMemoryBufferSize)
            {
                size = MaxMemoryBufferSize;
            }

            if (this.pool.TryDequeue(out var memoryStream))
            {
                return memoryStream;
            }

            return new MemoryStream(size);
        }

        public void Recycle(MemoryStream memoryBuffer)
        {
            if (memoryBuffer.Capacity > MaxMemoryBufferSize)
            {
                return;
            }

            if (this.pool.Count >= MaxPoolSize)
            {
                return;
            }

            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);

            this.pool.Enqueue(memoryBuffer);
        }

        /// <summary>
        /// 从ArrayPool租用字节数组，避免内存分配
        /// </summary>
        /// <param name="minimumLength">所需的最小长度</param>
        /// <returns>租用的字节数组，实际长度可能大于请求长度</returns>
        public static byte[] RentBuffer(int minimumLength)
        {
            return ByteArrayPool.Rent(minimumLength);
        }

        /// <summary>
        /// 归还字节数组到ArrayPool，必须与RentBuffer配对使用
        /// </summary>
        /// <param name="buffer">要归还的字节数组</param>
        /// <param name="clearArray">是否清空数组内容</param>
        public static void ReturnBuffer(byte[] buffer, bool clearArray = false)
        {
            ByteArrayPool.Return(buffer, clearArray);
        }
    }
}
#endif
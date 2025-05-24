using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    /// <summary>
    /// 循环缓冲区，用于网络数据的流式处理
    /// 支持数据的连续读写，自动管理内存块的分配和回收
    /// 使用ArrayPool<byte>优化内存使用，减少GC压力
    /// 实现Stream接口，支持标准的流操作
    /// 内部使用队列管理多个固定大小的内存块
    /// </summary>
    public class CircularBuffer : Stream
    {
        public int ChunkSize = 8192;

        private readonly Queue<byte[]> bufferQueue = new Queue<byte[]>();
        private readonly Queue<byte[]> bufferCache = new Queue<byte[]>();
        private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;

        public int LastIndex { get; set; }
        public int FirstIndex { get; set; }
        
        private byte[] lastBuffer;

        public CircularBuffer()
        {
            this.AddLast();
        }

        public override long Length
        {
            get
            {
                int c = 0;
                if (this.bufferQueue.Count == 0)
                {
                    c = 0;
                }
                else
                {
                    c = (this.bufferQueue.Count - 1) * ChunkSize + this.LastIndex - this.FirstIndex;
                }
                if (c < 0)
                {
                    Log.GetLogger().Error("CircularBuffer count < 0: {0}, {1}, {2}".Fmt(this.bufferQueue.Count, this.LastIndex, this.FirstIndex));
                }
                return c;
            }
        }

        public void AddLast()
        {
            byte[] buffer;
            if (this.bufferCache.Count > 0)
            {
                buffer = this.bufferCache.Dequeue();
            }
            else
            {
                buffer = ArrayPool.Rent(ChunkSize);
            }
            this.bufferQueue.Enqueue(buffer);
            this.lastBuffer = buffer;
        }

        public void RemoveFirst()
        {
            var buffer = bufferQueue.Dequeue();
            if (this.bufferCache.Count < 10) // 保持适量的缓存，避免频繁申请内存
            {
                this.bufferCache.Enqueue(buffer);
            }
            else
            {
                ArrayPool.Return(buffer);
            }
        }

        public byte[] First
        {
            get
            {
                if (this.bufferQueue.Count == 0)
                {
                    this.AddLast();
                }
                return this.bufferQueue.Peek();
            }
        }

        public byte[] Last
        {
            get
            {
                if (this.bufferQueue.Count == 0)
                {
                    this.AddLast();
                }
                return this.lastBuffer;
            }
        }

        /// <summary>
        /// 使用Span<T>进行高效的数据读取，避免内存拷贝
        /// </summary>
        public void Read(Stream stream, int count)
        {
            if (count > this.Length)
            {
                throw new Exception($"bufferList length < count, {Length} {count}");
            }

            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                int n = count - alreadyCopyCount;
                int availableInCurrentChunk = ChunkSize - this.FirstIndex;
                
                if (availableInCurrentChunk > n)
                {
                    var span = new ReadOnlySpan<byte>(this.First, this.FirstIndex, n);
                    stream.Write(span);
                    this.FirstIndex += n;
                    alreadyCopyCount += n;
                }
                else
                {
                    var span = new ReadOnlySpan<byte>(this.First, this.FirstIndex, availableInCurrentChunk);
                    stream.Write(span);
                    alreadyCopyCount += availableInCurrentChunk;
                    this.FirstIndex = 0;
                    this.RemoveFirst();
                }
            }
        }
        
        /// <summary>
        /// 使用Span<T>进行高效的数据写入，避免内存拷贝
        /// </summary>
        public void Write(Stream stream)
        {
            int count = (int)(stream.Length - stream.Position);
            
            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                if (this.LastIndex == ChunkSize)
                {
                    this.AddLast();
                    this.LastIndex = 0;
                }

                int n = count - alreadyCopyCount;
                int availableInCurrentChunk = ChunkSize - this.LastIndex;
                
                if (availableInCurrentChunk > n)
                {
                    var span = new Span<byte>(this.lastBuffer, this.LastIndex, n);
                    stream.ReadExactly(span);
                    this.LastIndex += n;
                    alreadyCopyCount += n;
                }
                else
                {
                    var span = new Span<byte>(this.lastBuffer, this.LastIndex, availableInCurrentChunk);
                    stream.ReadExactly(span);
                    alreadyCopyCount += availableInCurrentChunk;
                    this.LastIndex = ChunkSize;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
            {
                throw new Exception($"bufferList length < count, buffer length: {buffer.Length} offset: {offset} count: {count}");
            }

            long length = this.Length;
            if (length == 0)
            {
                return 0;
            }

            int n = (int)Math.Min(count, length);

            if (ChunkSize - this.FirstIndex >= n)
            {
                Array.Copy(this.First, this.FirstIndex, buffer, offset, n);
                this.FirstIndex += n;
                if (this.FirstIndex == ChunkSize)
                {
                    this.FirstIndex = 0;
                    this.RemoveFirst();
                }
                return n;
            }

            int alreadyCopyCount = 0;
            while (alreadyCopyCount < n)
            {
                int countToCopy = Math.Min(n - alreadyCopyCount, ChunkSize - this.FirstIndex);
                Array.Copy(this.First, this.FirstIndex, buffer, offset + alreadyCopyCount, countToCopy);
                alreadyCopyCount += countToCopy;
                this.FirstIndex += countToCopy;
                if (this.FirstIndex == ChunkSize)
                {
                    this.FirstIndex = 0;
                    this.RemoveFirst();
                }
            }

            return n;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                if (this.LastIndex == ChunkSize)
                {
                    this.AddLast();
                    this.LastIndex = 0;
                }

                int n = count - alreadyCopyCount;
                if (ChunkSize - this.LastIndex > n)
                {
                    Array.Copy(buffer, offset + alreadyCopyCount, this.lastBuffer, this.LastIndex, n);
                    this.LastIndex += n;
                    alreadyCopyCount += n;
                }
                else
                {
                    Array.Copy(buffer, offset + alreadyCopyCount, this.lastBuffer, this.LastIndex, ChunkSize - this.LastIndex);
                    alreadyCopyCount += ChunkSize - this.LastIndex;
                    this.LastIndex = ChunkSize;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 归还所有租用的数组
                while (this.bufferQueue.Count > 0)
                {
                    var buffer = this.bufferQueue.Dequeue();
                    ArrayPool.Return(buffer);
                }
                
                while (this.bufferCache.Count > 0)
                {
                    var buffer = this.bufferCache.Dequeue();
                    ArrayPool.Return(buffer);
                }
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Position { get; set; }
    }
}
#if UNITY_EDITOR || DOTNET
using System.Collections.Generic;
using System.IO;

namespace ET
{
    public class MemoryStreamPool
    {
        [StaticField]
        public static MemoryStreamPool Instance = new();
        
        private const int MaxMemoryBufferSize = 1024;

        private readonly Queue<MemoryStream> pool = new();

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

            if (this.pool.Count == 0)
            {
                return new MemoryStream(size);
            }

            return pool.Dequeue();
        }

        public void Recycle(MemoryStream memoryBuffer)
        {
            if (memoryBuffer.Capacity > 1024)
            {
                return;
            }

            if (this.pool.Count > 10) // 这里不需要太大，其实Kcp跟Tcp,这里1就足够了
            {
                return;
            }

            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);

            this.pool.Enqueue(memoryBuffer);
        }

    }
}
#endif
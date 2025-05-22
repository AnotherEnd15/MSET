using System;
using System.Buffers;
using System.ComponentModel;
using System.IO;
using System.Linq;
using MemoryPack;
using MemoryPack.Compression;

namespace ET
{
    public static class MemoryPackHelper
    {
        public static byte[] Serialize(object message)
        {
            if (message is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }
            using var compressor = new BrotliCompressor();
            MemoryPackSerializer.Serialize(message.GetType(),compressor, message);
            return compressor.ToArray();
        }


        public static object Deserialize(Type type, byte[] bytes, int index, int count)
        {
            using var decompressor = new BrotliDecompressor();
            var buffer = decompressor.Decompress(bytes.AsSpan(index, count));
            var obj = MemoryPackSerializer.Deserialize(type, buffer);
            if (obj is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return obj;
        }

        public static T Clone<T>(T origin) where T : class
        {
            var copy = Serialize(origin);
            return Deserialize(typeof (T), copy, 0, copy.Length) as T;
        }

        public static void Init()
        {
            
        }
    }
}
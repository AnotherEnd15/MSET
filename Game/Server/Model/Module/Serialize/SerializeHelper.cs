using System.IO;
using System;
using System.Collections.Generic;
using MemoryPack.Formatters;

namespace ET
{
    public static class SerializeHelper
    {
	    public static object Clone(object obj)
	    {
		    byte[] bytes = SerializeHelper.Serialize(obj);
		    return SerializeHelper.Deserialize(obj.GetType(), bytes, 0, bytes.Length);
	    }
	    
	    public static object Clone(IMessage obj)
	    {
		    byte[] bytes = SerializeHelper.Serialize(obj);
		    return SerializeHelper.Deserialize(obj.GetType(), bytes, 0, bytes.Length);
	    }
	    
	    public static object Deserialize(Type type, byte[] bytes, int index, int count)
		{
			return MemoryPackHelper.Deserialize(type, bytes, index, count);
		}

        public static byte[] Serialize(object message)
		{
			return MemoryPackHelper.Serialize(message);
		}

        public static void Serialize(ushort opcode, object message, MemoryStream stream)
        {
	        var bytes =  MemoryPackHelper.Serialize(message);
	        stream.Write(bytes,0,bytes.Length);   
        }

        public static object Deserialize(ushort opcode,Type type, MemoryStream memoryStream)
        {
	        return MemoryPackHelper.Deserialize(type,
		        memoryStream.GetBuffer(),
		        (int)memoryStream.Position,
		        (int)(memoryStream.Length-memoryStream.Position)
		        );    
        }
    }
}
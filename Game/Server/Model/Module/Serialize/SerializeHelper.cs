using System.IO;
using System;
using System.Collections.Generic;
using MemoryPack.Formatters;

namespace ET
{
    public static class SerializeHelper
    {
        public static void Serialize(ushort opcode, object message, MemoryStream stream)
        {
	        if (opcode < OpcodeRangeDefine.InnerMaxOpcode && opcode > OpcodeRangeDefine.OuterMaxOpcode)
	        {
		        // 走bson序列化
		        MongoHelper.Serialize(message, stream);
		        return;
	        }
	        var bytes =  MemoryPackHelper.Serialize(message);
	        stream.Write(bytes,0,bytes.Length);   
        }

        public static object Deserialize(ushort opcode,Type type, MemoryStream memoryStream)
        {
	        if (opcode < OpcodeRangeDefine.InnerMaxOpcode && opcode > OpcodeRangeDefine.OuterMaxOpcode)
	        {
		        // 走bson序列化
		        return MongoHelper.Deserialize(type, memoryStream);
	        }
	        return MemoryPackHelper.Deserialize(type,
		        memoryStream.GetBuffer(),
		        (int)memoryStream.Position,
		        (int)(memoryStream.Length-memoryStream.Position)
		        );    
        }
    }
}
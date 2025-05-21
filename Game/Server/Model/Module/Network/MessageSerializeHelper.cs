using System;
using System.IO;

namespace ET
{
    public static class MessageSerializeHelper
    {
        private static MemoryStream GetStream(int count = 0)
        {
            MemoryStream stream = new MemoryStream(count);
            return stream;
        }

        public static (ushort, MemoryStream) MessageToStream(object message)
        {
            int headOffset = Packet.ActorIdLength;
            MemoryStream stream = GetStream(headOffset + Packet.OpcodeLength);

            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());

            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            stream.SetLength(headOffset + Packet.OpcodeLength);

            stream.GetBuffer().WriteTo(headOffset, opcode);
            
            SerializeHelper.Serialize(opcode, message, stream);
            
            stream.Seek(0, SeekOrigin.Begin);
            return (opcode, stream);
        }
    }
}
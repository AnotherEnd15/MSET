using System;

namespace ET
{
    public static class MessageSerializeHelper
    {

        public static (ushort, byte[]) MessageToStream(object message)
        {
            int headOffset = Packet.KcpOpcodeIndex;

            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            
            var rawBytes = SerializeHelper.Serialize(message);
            
            var finalBytes = new byte[rawBytes.Length + Packet.OpcodeLength];
            finalBytes.WriteTo(headOffset,opcode);
            rawBytes.CopyTo(finalBytes,Packet.OpcodeLength);

            return (opcode, finalBytes);
        }
    }
}
using System;

namespace ET
{
    public static class SerializeHelper
    {
	    public static object Clone(object obj)
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
    }
}
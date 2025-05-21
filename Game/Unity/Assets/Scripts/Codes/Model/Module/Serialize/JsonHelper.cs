#if !ROBOT
using System;
using LitJson;

namespace ET
{
    public static class JsonHelper
    {
        static JsonHelper()
        {
            JsonMapper.RegisterExporter<FixMath.fp>((t, w) =>
            {
                w.Write(t.Float);
            });
        }

        public static JsonData FromJson(string json)
        {
            return JsonMapper.ToObject(json);
        }
        
        public static string ToJson(this object obj)
        {
            return JsonMapper.ToJson(obj);
        }
    }
}
#endif
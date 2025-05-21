namespace ET
{
    public static class JsonHelper
    {
        public static string ToJson(this object obj)
        {
            return obj.ToString();
        }
    }
}
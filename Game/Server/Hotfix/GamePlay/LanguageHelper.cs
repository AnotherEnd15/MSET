using System.Collections.Generic;

namespace ET.Server
{
    public static class LanguageHelper
    {
        public static string GetLanguage(this int key)
        {
            var result = string.Empty;
            switch (Options.Instance.LanguageType)
            {
                case 0:
                    result = LanguageConfigCategory.Instance.Get(key).CN;
                    break;
                default:
                    result = LanguageConfigCategory.Instance.Get(key).CN;
                    break;
            }

            if (string.IsNullOrEmpty(result))
            {
                return result;
            }

            return result;
        }
        
        public static string GetLanguage<T>(this int key,List<T> args)
        {
            var ret = key.GetLanguage();
            var array = new object[args.Count];
            for (int i = 0; i < args.Count; i++)
            {
                array[i] = args[i];
            }
            return string.Format(ret, array);
        }

        public static string GetLanguage(this int? key,params object[] args)
        {
            if (key == null)
                return "";
            var ret = key.GetLanguage();
            return string.Format(ret, args);
        }
        public static string GetLanguage(this int key, params object[] args)
        {
            var ret = key.GetLanguage();
            return string.Format(ret, args);
        }


        public static string GetLanguage(this int? key)
        {
            if (key == null)
                return "";
            return key.Value.GetLanguage();
        }
    }
}
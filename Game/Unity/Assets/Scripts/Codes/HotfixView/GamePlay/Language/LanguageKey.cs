using System.Collections.Generic;
using System.Linq;

namespace ET
{
    public static class Language
    {
        public static string GetLanguage(this int key)
        {
            if (!LanguageConfigCategory.Instance.DataMap.TryGetValue(key,out var languageConfig))
            {
                return key.ToString();
            }

            var result = string.Empty;
            switch (PlayerSetting.Instance.LanguageType)
            {
                case LanguageType.CN:
                    result = languageConfig.CN;
                    break;
                default:
                    result = key.ToString();
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
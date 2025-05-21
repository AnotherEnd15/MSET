using System;
using System.Collections.Generic;

namespace ET
{
    public static class ShortNumberHelper
    {
        [StaticField]
        private static List<Tuple<int,int, string>> ZeroesAndLetters = new ()
        {
            // 几位数字开始压缩 压缩时除以多少 后缀字母
            new Tuple<int,int, string>(18,15, "Q"),
            new Tuple<int,int, string>(15,12, "T"),
            new Tuple<int,int, string>(12,9, "B"),
            new Tuple<int,int, string>(9,6, "M"),
            new Tuple<int,int, string>(6,3, "K"), // 一开始10000以内的数字不压缩
        };

        public static string GetPointsShortened(long num)
        {
            int zeroCount = num.ToString().Length;
            for (int i = 0; i < ZeroesAndLetters.Count; i++)
                if (zeroCount >= ZeroesAndLetters[i].Item1)
                    return (num / Math.Pow(10, ZeroesAndLetters[i].Item2)).ToString("F") + ZeroesAndLetters[i].Item3;
            return num.ToString();
        }
        
        public static string GetPointsShortened(decimal num)
        {
            var str = GetIntegerDigitStr(num);
            int zeroCount = str.Length;
            for (int i = 0; i < ZeroesAndLetters.Count; i++)
            {
                if (zeroCount >= ZeroesAndLetters[i].Item1)
                {
                    decimal unit = (decimal)Math.Pow(10, ZeroesAndLetters[i].Item2);
                    return (num / unit).ToString("F") + ZeroesAndLetters[i].Item3;
                }
            }
            return str;
        }
        
        static string GetIntegerDigitStr(decimal number)
        {
            string numberStr = number.ToString(); // 转换为字符串
            int index = numberStr.IndexOf('.');    // 查找小数点的索引
            if (index != -1)
            {
                // 若存在小数点，则去除小数部分
                numberStr = numberStr.Substring(0, index);
            }
            return numberStr; // 返回整数位数
        }
    }
}
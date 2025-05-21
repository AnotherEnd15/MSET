using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ET
{
	public static class StringHelper
	{
		public static IEnumerable<byte> ToBytes(this string str)
		{
			byte[] byteArray = Encoding.Default.GetBytes(str);
			return byteArray;
		}
        public static string BytesToStringBySplit(this byte[] args, char split = ',')
        {
            if (args == null)
            {
                return "";
            }

            string argStr =string.Empty;
            for (int arrIndex = 0; arrIndex < args.Length; arrIndex++)
            {
                argStr += args[arrIndex];
                if (arrIndex != args.Length - 1)
                {
                    argStr += split;
                }
            }

            return argStr;

        }
        public static byte[] ToByteArrayBySplit(this string str, char split = ',')
        {
            var strs = str.Split(split);
            byte[] ret = new byte[strs.Length];
            for (int i = 0; i < strs.Length; i++)
            {
                ret[i] = byte.Parse(strs[i]);
            }

            return ret;
        }
		public static string BytesToString(this byte[] bytes)
		{
            return Encoding.Default.GetString(bytes);
        }
        public static byte[] ToByteArray(this string str)
		{
			byte[] byteArray = Encoding.Default.GetBytes(str);
			return byteArray;
		}

	    public static byte[] ToUtf8(this string str)
	    {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            return byteArray;
        }

		public static byte[] HexToBytes(this string hexString)
		{
			if (hexString.Length % 2 != 0)
			{
				throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
			}

			var hexAsBytes = new byte[hexString.Length / 2];
			for (int index = 0; index < hexAsBytes.Length; index++)
			{
				string byteValue = "";
				byteValue += hexString[index * 2];
				byteValue += hexString[index * 2 + 1];
				hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			}
			return hexAsBytes;
		}

		public static string Fmt(this string text, params object[] args)
		{
			return string.Format(text, args);
		}

		public static string ListToString<T>(this List<T> list)
		{
			return "[" + string.Join(",", list) + "]";
		}
		
		public static string ArrayToString<T>(this T[] args)
		{
			if (args == null)
			{
				return "";
			}

			string argStr = " [";
			for (int arrIndex = 0; arrIndex < args.Length; arrIndex++)
			{
				argStr += args[arrIndex];
				if (arrIndex != args.Length - 1)
				{
					argStr += ", ";
				}
			}

			argStr += "]";
			return argStr;
		}
		public static string ArrayToString<T>(this T[] args, int index, int count)
		{
			if (args == null)
			{
				return "";
			}

			string argStr = " [";
			for (int arrIndex = index; arrIndex < count + index; arrIndex++)
			{
				argStr += args[arrIndex];
				if (arrIndex != args.Length - 1)
				{
					argStr += ", ";
				}
			}

			argStr += "]";
			return argStr;
		}

		public static string Bytes2Str(this long bytes)
		{
			if (bytes < 1024)
			{
				return $"{bytes} B";
			}

			if (bytes < 1024 * 1024)
			{
				return $"{(bytes/(decimal)1024):F} KB";
			}

			return $"{(bytes / (decimal)(1024 * 1024)):F} MB";
		}

		public static int[] ToIntArrayBySplit(this string str, char split = ',')
		{
			var strs = str.Split(split);
			int[] ret = new int[strs.Length];
			for (int i = 0; i < strs.Length; i++)
			{
				ret[i] = int.Parse(strs[i]);
			}

			return ret;
		}

		public static int GetLength(string inputString)
		{
			System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
			int tempLen = 0;
			byte[] s = ascii.GetBytes(inputString);
			for (int i = 0; i < s.Length; i++)
			{
				if ((int)s[i] == 63)
					tempLen += 2;
				else
					tempLen += 1;
			}
			return tempLen;
		}
	}
}
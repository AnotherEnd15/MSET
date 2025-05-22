#if UNITY_EDITOR || DOTNET
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ET
{
	public static class MD5Helper
	{
		public static string FileMD5(string filePath)
		{
			byte[] retVal;
            using (FileStream file = new FileStream(filePath, FileMode.Open))
            {
	            MD5 md5 = MD5.Create();
				retVal = md5.ComputeHash(file);
			}
			return retVal.ToHex("x2");
		}
		
		public static string StringMD5(string content)
		{
			MD5 md5 = MD5.Create();
			return md5.ComputeHash(Encoding.UTF8.GetBytes(content)).ToHex("x2");
		}
	}
}
#endif

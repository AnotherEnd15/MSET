using System.Net;

namespace ET
{
public static class HttpServerHelper
    {
        /// <summary>
        /// 响应http请求
        /// </summary>
        public static void Response(HttpListenerContext context, int statusCode, string contentType, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            byte[] outBytes = context.Request.ContentEncoding.GetBytes(content);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = contentType;
            context.Response.ContentLength64 = outBytes.Length;
            context.Response.ContentEncoding = context.Request.ContentEncoding;
            context.Response.OutputStream.Write(outBytes, 0, outBytes.Length);
        }

        public static string GetContent(HttpListenerContext context)
        {
            var stream = context.Request.InputStream;
            byte[] buffer = new byte[(int)context.Request.ContentLength64];
            stream.ReadExactly(buffer, 0, (int)context.Request.ContentLength64);
            string content = context.Request.ContentEncoding.GetString(buffer, 0, buffer.Length);
            return content;
        }
    }
}
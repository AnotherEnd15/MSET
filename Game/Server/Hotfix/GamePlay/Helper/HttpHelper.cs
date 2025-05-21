using System.Net;
using System.Text;

namespace ET.Server
{
    public static class HttpHelper
    {
        public static void Response(HttpListenerContext context, object response)
        {
            byte[] bytes = JsonHelper.ToJson(response).ToUtf8();
            context.Response.StatusCode = 200;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }
        
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
    }
}
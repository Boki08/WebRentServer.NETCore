using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace WebRentServer.NETCore.ETagHelper
{
    
    // prevents the action filter methods to be invoked twice
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class ETagFilter : ActionFilterAttribute, IAsyncActionFilter
    {
        private readonly int[] _statusCodes;

        public ETagFilter(params int[] statusCodes)
        {
            _statusCodes = statusCodes;
            if (statusCodes.Length == 0) _statusCodes = new[] { StatusCodes.Status200OK, StatusCodes.Status201Created };
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {

        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (((context.Result as ObjectResult)?.StatusCode)!= null && _statusCodes.Any(x=>x==(context.Result as ObjectResult)?.StatusCode))
            {
                var content = JsonConvert.SerializeObject((context.Result as ObjectResult)?.Value);

                var etag = ETagGenerator.GenerateETag(Encoding.UTF8.GetBytes(content));

                if (context.HttpContext.Request.Headers.Keys.Contains(HeaderNames.IfMatch) && context.HttpContext.Request.Headers[HeaderNames.IfMatch].ToString() == etag)
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
                }
                context.HttpContext.Response.Headers.Add(HeaderNames.ETag, etag);
            }
            else
            {
                context.HttpContext.Response.Headers.Add(HeaderNames.ETag, context.HttpContext.Request.Headers[HeaderNames.IfMatch].ToString());
            }
        }
    }
    // Helper class that generates the etag from a key (route) and content (response)
    public static class ETagGenerator
    {
        //public static string GetETag(string key, byte[] contentBytes)
        //{
        //    var keyBytes = Encoding.UTF8.GetBytes(key);
        //    var combinedBytes = Combine(keyBytes, contentBytes);

        //    return GenerateETag(combinedBytes);
        //}

        public static string GenerateETag(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                string hex = BitConverter.ToString(hash);
                return hex.Replace("-", "");
            }
        }

        //private static byte[] Combine(byte[] a, byte[] b)
        //{
        //    byte[] c = new byte[a.Length + b.Length];
        //    Buffer.BlockCopy(a, 0, c, 0, a.Length);
        //    Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
        //    return c;
        //}
    }
}

using Microsoft.Net.Http.Headers;
using System.Text;

namespace WebRentServer.NETCore.ETagHelper
{
    public static class ModifiableResourceExtensions
    {
        public static string GetWeakETag(this IModifiableResource resource, string obj) => ETagGenerator.GenerateETag(Encoding.UTF8.GetBytes(obj));

        public static bool HasPreconditionFailed(this IModifiableResource resource, HttpRequest request)
        {
            bool preconditionFailed = false;

            if (HttpMethods.IsPut(request.Method) || HttpMethods.IsPatch(request.Method))
            {
                if (request.Headers.Keys.Contains(HeaderNames.IfMatch))
                {
                    if (request.Headers[HeaderNames.IfMatch].ToString() != resource.ETag)
                    {
                        preconditionFailed = true;
                    }
                }
                else
                    preconditionFailed = true;
            }

            return preconditionFailed;
        }
    }
}
using System.Net;
using System.Text;

namespace Espada.Tests.Common.Http
{
    public static class HttpResponseFactory
    {
        public static HttpResponseMessage Json(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            ArgumentNullException.ThrowIfNull(json);

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
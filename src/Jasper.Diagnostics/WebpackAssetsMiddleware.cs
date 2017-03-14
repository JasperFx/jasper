using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jasper.Diagnostics
{
    public class WebpackAssetsMiddleware
    {
        private readonly RequestDelegate _next;

        public WebpackAssetsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);
        }
    }

    public class WebpackAssets
    {
    }
}

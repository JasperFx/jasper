using Microsoft.AspNetCore.Builder;

namespace Jasper.WebSockets
{
    public static class JasperWebSocketMiddlewareExtensions
    {
        public static IApplicationBuilder UseJasperWebSockets(this IApplicationBuilder app)
        {
            var transport = app.ApplicationServices.GetService(typeof(WebSocketTransport));

            return app
                .UseWebSockets()
                .UseMiddleware<JasperWebSocketMiddleware>(transport);


        }
    }
}

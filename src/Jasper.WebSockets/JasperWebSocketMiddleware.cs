using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jasper.WebSockets
{
    public class JasperWebSocketMiddleware
    {
        private readonly WebSocketTransport _transport;
        private readonly RequestDelegate _next;

        public JasperWebSocketMiddleware(WebSocketTransport transport, RequestDelegate next)
        {
            _transport = transport;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await _transport.Accept(socket);
        }
    }
}
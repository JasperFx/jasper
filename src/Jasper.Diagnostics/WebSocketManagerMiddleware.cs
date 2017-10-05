using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jasper.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Jasper.Diagnostics
{
    public static class WebSocketMiddlewareExtensions
    {
        public static IApplicationBuilder MapWebSocket(
            this IApplicationBuilder app,
            PathString path,
            SocketConnection handler,
            ISocketConnectionManager manager)
        {
            return app.Map(path, _app => _app.UseMiddleware<WebSocketManagerMiddleware>(handler, manager));
        }
    }

    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SocketConnection _handler;
        private readonly ISocketConnectionManager _manager;

        public WebSocketManagerMiddleware(
            RequestDelegate next,
            SocketConnection handler,
            ISocketConnectionManager manager)
        {
            _next = next;
            _handler = handler;
            _manager = manager;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            _manager.Add(socket);
            await _handler.OnConnected(socket);

            await receive(socket, async(result, message) =>
            {
                if(result.MessageType == WebSocketMessageType.Text)
                {
                    await _handler.ReceiveAsync(socket, message);
                    return;
                }
                else if(result.MessageType == WebSocketMessageType.Close)
                {
                    await _handler.OnDisconnected(socket);
                    await _manager.Remove(socket);
                    return;
                }
            });
        }

        private async Task receive(WebSocket socket, Action<WebSocketReceiveResult, string> handleMessage)
        {
            while (socket.State == WebSocketState.Open)
            {
                var token = CancellationToken.None;

                var buffer = new ArraySegment<Byte>(new Byte[100000]);
                var received = await socket.ReceiveAsync(buffer, token);

                var json = buffer.ReadString(received);

                if (received.EndOfMessage)
                {
                    handleMessage(received, json);
                }
                else
                {
                    var builder = new StringBuilder(json);

                    while (!received.EndOfMessage)
                    {
                        received = await socket.ReceiveAsync(buffer, token);
                        json = buffer.ReadString(received);

                        builder.Append(json);
                    }

                    handleMessage(received, builder.ToString());
                }
            }
        }
    }

    public static class ArraySegmentExtensions
    {
        public static string ReadString(this ArraySegment<byte> buffer, WebSocketReceiveResult result)
        {
            {
                return Encoding.UTF8.GetString(buffer.Array,
                    buffer.Offset,
                    result.Count);
            }
        }
    }
}

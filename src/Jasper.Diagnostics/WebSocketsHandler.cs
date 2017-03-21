using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Jasper.Remotes.Messaging;

namespace Jasper.Diagnostics
{
    public interface IDiagnosticsClient
    {
        void Send<T>(T message);
    }

    public class DiagnosticsClient : IDiagnosticsClient
    {
        private readonly ISocketConnectionManager _manager;

        public DiagnosticsClient(ISocketConnectionManager manager)
        {
            _manager = manager;
        }

        public void Send<T>(T message)
        {
            var json = JsonSerialization.ToJson(message, true);
            _manager.SendToAllAsync(json).ConfigureAwait(false);
        }
    }

    public interface ISocketConnection
    {
        Task OnConnected(WebSocket socket);
        Task OnDisconnected(WebSocket socket);
        Task RecieveAsync(WebSocket socket, string text);
    }

    public class SocketConnection : ISocketConnection
    {
        private readonly Func<WebSocket, string, Task> _recieved;
        private readonly Func<WebSocket, Task> _onConnected;
        private readonly Func<WebSocket, Task> _onDisconnected;

        public SocketConnection(
            Func<WebSocket, string, Task> recieved,
            Func<WebSocket, Task> onConnected = null,
            Func<WebSocket, Task> onDisconnected = null)
        {
            _recieved = recieved;
            _onConnected = onConnected ?? (s => Task.CompletedTask);
            _onDisconnected = onDisconnected ?? (s => Task.CompletedTask);
        }

        public Task OnConnected(WebSocket socket)
        {
            return _onConnected(socket);
        }

        public Task OnDisconnected(WebSocket socket)
        {
            return _onDisconnected(socket);
        }

        public Task RecieveAsync(WebSocket socket, string text)
        {
            return _recieved(socket, text);
        }
    }

    public static class WebSocketMiddlewareExtensions
    {
        public static IApplicationBuilder MapWebSocket(
            this IApplicationBuilder app,
            PathString path,
            ISocketConnection handler,
            ISocketConnectionManager manager)
        {
            return app.Map(path, _app => _app.UseMiddleware<WebSocketManagerMiddleware>(handler, manager));
        }
    }

    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISocketConnection _handler;
        private readonly ISocketConnectionManager _manager;

        public WebSocketManagerMiddleware(
            RequestDelegate next,
            ISocketConnection handler,
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
                    await _handler.RecieveAsync(socket, message);
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

    public interface ISocketConnectionManager
    {
        WebSocket GetSocketById(string id);
        string GetId(WebSocket socket);
        string Add(WebSocket socket);
        Task Remove(WebSocket socket);
        Task SendAsync(string socketId, string text);
        Task SendToAllAsync(string text);
    }

    public class SocketConnectionManager : ISocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public WebSocket GetSocketById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key == id).Value;
        }

        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        public string GetId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        public string Add(WebSocket socket)
        {
            var id = CreateConnectionId();
            if(_sockets.TryAdd(CreateConnectionId(), socket))
            {
                return id;
            }
            return null;
        }

        public async Task Remove(WebSocket socket)
        {
            var id = GetId(socket);
            WebSocket s;
            _sockets.TryRemove(id, out s);

            await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                statusDescription: "Closed by the WebSocketManager",
                cancellationToken: CancellationToken.None);
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket == null || socket.State != WebSocketState.Open)
            {
                return;
            }

            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.UTF8.GetBytes(message),
                    offset: 0,
                    count: message.Length),
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None);
        }

        public async Task SendAsync(string socketId, string text)
        {
            await SendMessageAsync(GetSocketById(socketId), text);
        }

        public async Task SendToAllAsync(string text)
        {
            foreach (var socket in _sockets.Values)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await SendMessageAsync(socket, text);
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

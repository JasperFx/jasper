using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Diagnostics
{
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
}

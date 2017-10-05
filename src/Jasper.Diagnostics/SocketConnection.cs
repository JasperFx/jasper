using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Jasper.Diagnostics
{

    public class SocketConnection
    {
        private readonly Func<WebSocket, string, Task> _received;
        private readonly Func<WebSocket, Task> _onConnected;
        private readonly Func<WebSocket, Task> _onDisconnected;

        public SocketConnection(
            Func<WebSocket, string, Task> received,
            Func<WebSocket, Task> onConnected = null,
            Func<WebSocket, Task> onDisconnected = null)
        {
            _received = received;
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

        public Task ReceiveAsync(WebSocket socket, string text)
        {
            return _received(socket, text);
        }
    }
}

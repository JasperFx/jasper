using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Jasper.Diagnostics
{
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
}

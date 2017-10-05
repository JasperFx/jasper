using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jasper.WebSockets
{
    public static class WebSocketExtensions
    {
        public static string ReadString(this ArraySegment<byte> buffer, WebSocketReceiveResult result)
        {
            {
                return Encoding.UTF8.GetString(buffer.Array,
                    buffer.Offset,
                    result.Count);
            }
        }

        public static Task SendMessageAsync(this WebSocket socket, string message)
        {
            if (socket == null || socket.State != WebSocketState.Open)
            {
                return Task.CompletedTask;
            }

            return socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.UTF8.GetBytes(message),
                    offset: 0,
                    count: message.Length),
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None);
        }
    }
}
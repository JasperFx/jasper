using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.WorkerQueues;

namespace Jasper.WebSockets
{
    public class WebSocketCollection : IDisposable
    {
        private readonly IWorkerQueue _queue;
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public WebSocketCollection(IWorkerQueue queue)
        {
            _queue = queue;
        }

        public Task Enqueue(Envelope envelope)
        {
            // TODO -- be more specific w/ the destination here. May choose to send back to a specific
            // socket instead of global

            var json = envelope.Message.ToCleanJson();

            if (envelope.Destination == WebSocketTransport.DefaultUri)
            {
                return SendJsonToAll(json);
            }
            else
            {
                throw new NotSupportedException("WebSocket transport doesn't yet know how to send to a specific socket");
            }
        }

        public Task SendJsonToAll(string json)
        {
            var all = _sockets.Values.Where(x => x.State == WebSocketState.Open).ToArray();
            var tasks = all.Select(x => x.SendMessageAsync(json));

            return Task.WhenAll(tasks);
        }

        public async Task Accept(WebSocket socket)
        {
            var id = Guid.NewGuid().ToString();
            _sockets[id] = socket;

            while (socket.State == WebSocketState.Open)
            {
                var token = CancellationToken.None;

                var buffer = new ArraySegment<Byte>(new Byte[100000]);
                var received = await socket.ReceiveAsync(buffer, token);

                switch (received.MessageType)
                {
                    case WebSocketMessageType.Text:
                        var json = await readJson(socket, buffer, received, token);

                        await handleJson(id, json);
                        break;

                    case WebSocketMessageType.Close:
                        removeSocket(id);
                        break;
                }
            }

            removeSocket(id);
        }

        private static async Task<string> readJson(WebSocket socket, ArraySegment<byte> buffer, WebSocketReceiveResult received,
            CancellationToken token)
        {
            var json = buffer.ReadString(received);

            if (received.EndOfMessage) return json;

            var builder = new StringBuilder(json);

            while (!received.EndOfMessage)
            {
                received = await socket.ReceiveAsync(buffer, token);
                json = buffer.ReadString(received);

                builder.Append(json);
            }

            json = builder.ToString();

            return json;
        }

        private Task handleJson(string socketId, string json)
        {

            var message = JsonSerialization.DeserializeMessage(json);
            var envelope = new Envelope(message)
            {
                ReplyUri = WebSocketTransport.DefaultUri,
                ReceivedAt = WebSocketTransport.DefaultUri,
                Callback = new LightweightCallback(_queue)
            };

            return _queue.Enqueue(envelope);
        }

        private void removeSocket(string id)
        {
            try
            {
                _sockets.TryRemove(id, out var removed);
            }
            catch (Exception)
            {
                // yep, that's right, do nothing here
            }
        }



        public void Dispose()
        {
            foreach (var socket in _sockets.Values.ToArray())
            {
                socket.Dispose();
            }

            _sockets.Clear();
        }
    }
}

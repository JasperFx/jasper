using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;

namespace Jasper.WebSockets
{
    public class WebSocketTransport : ITransport
    {
        private readonly HandlerGraph _handlers;
        public static readonly Uri DefaultUri = "ws://default".ToUri();

        public WebSocketTransport(HandlerGraph handlers)
        {
            _handlers = handlers;
        }

        public void Dispose()
        {
            foreach (var socket in _sockets.Values.ToArray())
            {
                socket.Dispose();
            }

            _sockets.Clear();
        }

        public string Protocol { get; } = "ws";

        public Task Send(Envelope envelope, Uri destination)
        {
            // TODO -- be more specific w/ the destination here. May choose to send back to a specific
            // socket instead of global

            var json = envelope.Message.ToCleanJson();

            if (destination == DefaultUri)
            {
                return sendJsonToAll(json);
            }
            else
            {
                throw new NotSupportedException("WebSocket transport doesn't yet know how to send to a specific socket");
            }
        }

        public Task SendToAll(ClientMessage message)
        {
            var json = message.ToCleanJson();
            return sendJsonToAll(json);
        }

        private Task sendJsonToAll(string json)
        {
            var all = _sockets.Values.Where(x => x.State == WebSocketState.Open).ToArray();
            var tasks = all.Select(x => x.SendMessageAsync(json));

            return Task.WhenAll(tasks);
        }

        public IChannel[] Start(IHandlerPipeline pipeline, BusSettings settings, OutgoingChannels channels)
        {
            _pipeline = pipeline;

            foreach (var messageType in _handlers.Chains.Select(x => x.MessageType).Where(x => x.CanBeCastTo<ClientMessage>()))
            {
                JsonSerialization.RegisterType(messageType.ToMessageAlias(), messageType);
            }

            _retries = channels.DefaultRetryChannel;

            return new IChannel[]{new OutgoingWebSocketChannel(this)};
        }

        Uri ITransport.DefaultReplyUri()
        {
            return DefaultUri;
        }

        public TransportState State { get; } = TransportState.Enabled;

        public void Describe(TextWriter writer)
        {
            writer.WriteLine("Listening for WebSockets messages");
        }

        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();
        private IHandlerPipeline _pipeline;
        private IChannel _retries;

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
            if (_pipeline == null) return Task.CompletedTask;

            var message = JsonSerialization.DeserializeMessage(json);
            var envelope = new Envelope(message) {Callback = new WebSocketCallback(_retries)};

            return _pipeline.Invoke(envelope);
        }

        public Task SendToAll(Envelope envelope)
        {
            var json = envelope.Message.ToCleanJson();
            return sendJsonToAll(json);
        }
    }
}

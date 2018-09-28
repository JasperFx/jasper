using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;

namespace Jasper.WebSockets
{
    public class WebSocketTransport : ITransport
    {
        private readonly HandlerGraph _handlers;
        public static readonly Uri DefaultUri = "ws://default".ToUri();
        private WebSocketCollection _sockets;

        public WebSocketTransport(HandlerGraph handlers)
        {
            _handlers = handlers;

        }

        public void Dispose()
        {
            _sockets.Dispose();
        }

        public string Protocol { get; } = "ws";


        public Task SendToAll(ClientMessage message)
        {
            var json = message.ToCleanJson();
            return _sockets.SendJsonToAll(json);
        }

        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
            return new WebSocketSendingAgent(_sockets);
        }

        Uri ITransport.ReplyUri => DefaultUri;

        public void StartListening(IMessagingRoot root)
        {
            _sockets = new WebSocketCollection(root.Workers);

            foreach (var messageType in _handlers.Chains.Select(x => x.MessageType).Where(x => x.CanBeCastTo<ClientMessage>()))
            {
                JsonSerialization.RegisterType(messageType.ToMessageTypeName(), messageType);
            }
        }

        public void Describe(TextWriter writer)
        {
            writer.WriteLine("WebSocket transport is active");
        }

        // Ignored
        public ListeningStatus ListeningStatus { get; set; }

        public Task Accept(WebSocket socket)
        {
            return _sockets.Accept(socket);
        }

        public Task SendToAll(Envelope envelope)
        {
            var json = envelope.Message.ToCleanJson();
            return _sockets.SendJsonToAll(json);
        }
    }
}

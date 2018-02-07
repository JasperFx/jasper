using System;
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
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;
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

        public ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation)
        {
            return new WebSocketSendingAgent(_sockets);
        }

        Uri ITransport.LocalReplyUri => DefaultUri;

        public void StartListening(BusSettings settings, IWorkerQueue workers)
        {
            _sockets = new WebSocketCollection(workers);

            foreach (var messageType in _handlers.Chains.Select(x => x.MessageType).Where(x => x.CanBeCastTo<ClientMessage>()))
            {
                JsonSerialization.RegisterType(messageType.ToMessageAlias(), messageType);
            }
        }

        public void Describe(TextWriter writer)
        {
            writer.WriteLine("WebSocket transport is active");
        }

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

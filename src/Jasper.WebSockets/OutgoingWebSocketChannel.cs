using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Remotes.Messaging;

namespace Jasper.WebSockets
{
    public class OutgoingWebSocketChannel : IChannel
    {
        private readonly WebSocketTransport _transport;

        public OutgoingWebSocketChannel(WebSocketTransport transport)
        {
            _transport = transport;
        }

        public Uri Uri { get; } = WebSocketTransport.DefaultUri;
        public Uri ReplyUri { get; } = WebSocketTransport.DefaultUri;
        public Uri Destination { get; } = WebSocketTransport.DefaultUri;
        public Uri Alias { get; } = WebSocketTransport.DefaultUri;

        public string QueueName()
        {
            return "default";
        }

        public bool ShouldSendMessage(Type messageType)
        {
            return messageType.CanBeCastTo<ClientMessage>();
        }

        public Task Send(Envelope envelope)
        {
            return _transport.SendToAll(envelope);
        }
    }
}

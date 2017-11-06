using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Sending;

namespace Jasper.WebSockets
{
    public class WebSocketSendingAgent : ISendingAgent
    {
        private readonly WebSocketCollection _sockets;

        public WebSocketSendingAgent(WebSocketCollection sockets)
        {
            _sockets = sockets;
        }

        public void Dispose()
        {
            // nothing
        }

        public Uri Destination { get; } = WebSocketTransport.DefaultUri;
        public Uri DefaultReplyUri { get; set; }

        public Task EnqueueOutgoing(Envelope envelope)
        {
            var json = envelope.Message.ToCleanJson();
            return _sockets.SendJsonToAll(json);
        }

        public Task StoreAndForward(Envelope envelope)
        {
            return EnqueueOutgoing(envelope);
        }

        public void Start()
        {
            // nothing
        }
    }
}

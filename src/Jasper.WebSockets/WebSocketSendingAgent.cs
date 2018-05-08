using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;

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

        public bool Latched { get; } = false;

        public bool IsDurable => false;

        public Task EnqueueOutgoing(Envelope envelope)
        {
            var json = envelope.Message.ToCleanJson();
            return _sockets.SendJsonToAll(json);
        }

        public Task StoreAndForward(Envelope envelope)
        {
            return EnqueueOutgoing(envelope);
        }

        public async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                await EnqueueOutgoing(envelope);
            }
        }

        public void Start()
        {
            // nothing
        }

        public int QueuedCount { get; } = 0;
    }
}

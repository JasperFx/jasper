using System;
using System.Threading.Tasks.Dataflow;

namespace Jasper.WebSockets
{
    public class WebSocketSender : IDisposable, IWebSocketSender
    {
        private readonly WebSocketTransport _transport;
        private readonly ActionBlock<ClientMessage> _block;

        public WebSocketSender(WebSocketTransport transport)
        {
            _transport = transport;
            _block = new ActionBlock<ClientMessage>(m => _transport.SendToAll(m));
        }

        public void Send(ClientMessage message)
        {
            _block.Post(message);
        }

        public void Send(params ClientMessage[] messages)
        {
            foreach (var message in messages)
            {
                Send(message);
            }
        }

        public void Dispose()
        {
            _block.Complete();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using JasperBus.Configuration;
using JasperBus.Model;
using JasperBus.Runtime;

namespace JasperBus.Transports.InMemory
{
    public class InMemoryQueue : IDisposable
    {
        private readonly IDictionary<Uri, BufferBlock<InMemoryMessage>> _buffers = new Dictionary<Uri, BufferBlock<InMemoryMessage>>();
        private readonly InMemorySettings _settings;

        public InMemoryQueue(InMemorySettings settings)
        {
            _settings = settings;
        }

        public void Start(IEnumerable<ChannelNode> nodes)
        {
            try
            {
                foreach (var node in nodes)
                {
                    _buffers.Add(node.Uri, new BufferBlock<InMemoryMessage>());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task Send(byte[] data, IDictionary<string, string> headers, Uri destination)
        {
            var payload = new InMemoryMessage
            {
                Id = Guid.NewGuid(),
                Data = data,
                Headers = headers,
                SentAt = DateTime.UtcNow
            };

            await Send(payload, destination).ConfigureAwait(false);
        }

        public Task Send(InMemoryMessage message, Uri destination)
        {
            return _buffers[destination].SendAsync(message);
        }

        public async Task Delay(InMemoryMessage message, Uri destination, TimeSpan delayTime)
        {
            await Task.Delay(delayTime).ConfigureAwait(false);
            await Send(message, destination).ConfigureAwait(false);
        }

        public void ListenForMessages(Uri destination, IReceiver receiver)
        {
            _buffers[destination].LinkTo(new ActionBlock<InMemoryMessage>(message =>
            {
                return receiver.Receive(message.Data, message.Headers, new InMemoryCallback(this, message, destination));
            }), new DataflowLinkOptions { PropagateCompletion = true });
        }

        public void Dispose()
        {
            foreach (var buffer in _buffers)
            {
                buffer.Value.Complete();
            }
        }
    }
}

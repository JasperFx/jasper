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
        private readonly IDictionary<Uri, BroadcastBlock<InMemoryMessage>> _buffers = new Dictionary<Uri, BroadcastBlock<InMemoryMessage>>();
        private readonly InMemorySettings _settings;

        public InMemoryQueue(InMemorySettings settings)
        {
            _settings = settings;
        }

        public void Start(IEnumerable<ChannelNode> nodes)
        {
            try
            {
                var bufferOptions = new DataflowBlockOptions
                {
                    BoundedCapacity = _settings.BufferCapacity
                };

                foreach (var node in nodes)
                {
                    _buffers.Add(node.Uri, new BroadcastBlock<InMemoryMessage>(_ => _, bufferOptions));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Task Send(byte[] data, IDictionary<string, string> headers, Uri destination)
        {
            var payload = new InMemoryMessage
            {
                Id = Guid.NewGuid(),
                Data = data,
                Headers = headers,
                SentAt = DateTime.UtcNow
            };

            return Send(payload, destination);
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
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            var actionBlock = new ActionBlock<InMemoryMessage>(message =>
            {
                return receiver.Receive(message.Data, message.Headers, new InMemoryCallback(this, message, destination));
            });

            _buffers[destination].LinkTo(actionBlock, linkOptions);
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

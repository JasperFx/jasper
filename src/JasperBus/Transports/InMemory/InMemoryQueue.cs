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
        private readonly IDictionary<string, BufferBlock<InMemoryMessage>> _queue = new Dictionary<string, BufferBlock<InMemoryMessage>>();
        private readonly InMemorySettings _settings;
        private readonly int _port;
        public static readonly string ErrorQueueName = "errors";

        public InMemoryQueue(int port, InMemorySettings settings)
        {
            _port = port;
            _settings = settings;
        }

        public void Start(ChannelGraph channels, IEnumerable<ChannelNode> nodes)
        {
            try
            {
                foreach (var node in nodes)
                {
                    _queue.Add(node.Uri.Segments.Last(), new BufferBlock<InMemoryMessage>());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task Send(byte[] data, IDictionary<string, string> headers, Uri destination, string subQueue)
        {
            var payload = new InMemoryMessage
            {
                Id = Guid.NewGuid(),
                Data = data,
                Headers = headers,
                SentAt = DateTime.UtcNow,
                Queue = subQueue
            };

            await Send(payload, destination).ConfigureAwait(false);
        }

        public async Task Delay(InMemoryMessage message, Uri destination, TimeSpan delayTime)
        {
            await Task.Delay(delayTime).ConfigureAwait(false);
            await Send(message, destination).ConfigureAwait(false);
        }

        public Task Send(InMemoryMessage message, Uri destination)
        {
            return _queue[destination.Segments.Last()].SendAsync(message);
        }

        public void ListenForMessages(string subQueue, IReceiver receiver)
        {
            _queue[subQueue].LinkTo(new ActionBlock<InMemoryMessage>(message =>
            {
                receiver.Receive(message.Data, message.Headers, new InMemoryCallback(this, message));
            }), new DataflowLinkOptions { PropagateCompletion = true });
        }

        public void Dispose()
        {
            foreach (var q in _queue)
            {
                q.Value.Complete();
            }
        }
    }
}

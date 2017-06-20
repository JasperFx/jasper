using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Configuration;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Microsoft.Extensions.Options;

namespace Jasper.Bus.Transports.InMemory
{
    public class InMemoryQueue : IDisposable
    {
        private readonly IDictionary<Uri, BroadcastBlock<InMemoryMessage>> _buffers = new Dictionary<Uri, BroadcastBlock<InMemoryMessage>>();
        private readonly InMemorySettings _settings;

        public InMemoryQueue(InMemorySettings settings)
        {
            _settings = settings;
        }

        public void SendToReceiver(Uri destination, IReceiver receiver, InMemoryMessage message)
        {
            var callback = new InMemoryCallback(this, message, destination);

            if (message.Message == null)
            {
                receiver.Receive(message.Data, message.Headers, callback);
            }
            else
            {
                receiver.Receive(message.Message, message.Headers, callback);
            }
        }

        public void Start(IEnumerable<ChannelNode> nodes)
        {
            try
            {
                var bufferOptions = new DataflowBlockOptions
                {
                    BoundedCapacity = _settings.BufferCapacity,

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

        public Task Send(Envelope envelope, Uri destination)
        {
            var payload = InMemoryMessage.ForEnvelope(envelope);

            return Send(payload, destination);
        }

        public Task Send(InMemoryMessage message, Uri destination)
        {
            return _buffers[destination].SendAsync(message);
        }

        [Obsolete("Going to use the new delayed processor")]
        public async Task Delay(InMemoryMessage message, Uri destination, TimeSpan delayTime)
        {
            await Task.Delay(delayTime).ConfigureAwait(false);
            await Send(message, destination).ConfigureAwait(false);
        }


        public void Dispose()
        {
            foreach (var buffer in _buffers)
            {
                buffer.Value.Complete();
            }
        }

        public void ListenForMessages(ChannelNode node, IHandlerPipeline pipeline, ChannelGraph channels)
        {
            var receiver = new Receiver(pipeline, channels, node);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };


            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = node.MaximumParallelization
            };

            var actionBlock = new ActionBlock<InMemoryMessage>(message =>
            {
                SendToReceiver(node.Uri, receiver, message);
            }, options);

            _buffers[node.Uri].LinkTo(actionBlock, linkOptions);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Microsoft.Extensions.Options;

namespace Jasper.Bus.Transports.InMemory
{
    public class LoopbackQueue : IDisposable, ILoopbackQueue
    {
        private readonly IDictionary<Uri, BroadcastBlock<LoopbackMessage>> _buffers = new Dictionary<Uri, BroadcastBlock<LoopbackMessage>>();
        private readonly LoopbackSettings _settings;

        public LoopbackQueue(LoopbackSettings settings)
        {
            _settings = settings;
        }

        public Envelope EnvelopeForInlineMessage(object message)
        {
            var envelope = Envelope.ForMessage(message);
            envelope.Destination = LoopbackTransport.Retries;
            envelope.Callback = new LoopbackCallback(this, LoopbackMessage.ForEnvelope(envelope), LoopbackTransport.Retries);

            return envelope;
        }

        public void SendToReceiver(Uri destination, IReceiver receiver, LoopbackMessage message)
        {
            var callback = new LoopbackCallback(this, message, destination);
            message.Headers[Envelope.ReceivedAtKey] = destination.ToString();

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

                foreach (var node in nodes.Distinct())
                {
                    _buffers.Add(node.Uri, new BroadcastBlock<LoopbackMessage>(_ => _, bufferOptions));
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
            var payload = LoopbackMessage.ForEnvelope(envelope);

            return Send(payload, destination);
        }

        public Task Send(LoopbackMessage message, Uri destination)
        {
            return _buffers[destination].SendAsync(message);
        }

        [Obsolete("Going to use the new delayed processor")]
        public async Task Delay(LoopbackMessage message, Uri destination, TimeSpan delayTime)
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

            var actionBlock = new ActionBlock<LoopbackMessage>(message =>
            {
                SendToReceiver(node.Uri, receiver, message);
            }, options);

            _buffers[node.Uri].LinkTo(actionBlock, linkOptions);
        }
    }
}

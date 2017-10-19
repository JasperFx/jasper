using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Lightweight;
using Jasper.Util;

namespace Jasper.Bus.Transports.Loopback
{
    public class LoopbackTransport : ITransport, IQueueProvider
    {
        public static readonly string ProtocolName = "loopback";

        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly CompositeLogger _logger;
        private Lazy<IChannel> _retryChannel;

        private QueueCollection _queues;

        public LoopbackTransport(CompositeLogger logger)
        {
            _logger = logger;
        }

        public string Protocol => ProtocolName;

        public TransportState State { get; } = TransportState.Enabled;
        public void Describe(TextWriter writer)
        {
            if (_settings == null) return;
            foreach (var setting in _settings)
            {
                writer.WriteLine($"Processing messages at loopback queue '{setting.Name}'");
            }
        }

        public Task Send(Envelope envelope, Uri destination)
        {
            _queues.Enqueue(destination, envelope);

            return Task.CompletedTask;
        }

        public IChannel[] Start(IHandlerPipeline pipeline, BusSettings settings, OutgoingChannels channels)
        {
            _settings = settings.Loopback;
            _retryChannel = new Lazy<IChannel>(() => channels.DefaultRetryChannel);
            return startListeners(pipeline, settings).ToArray();
        }

        private IEnumerable<IChannel> startListeners(IHandlerPipeline pipeline, BusSettings settings)
        {
            _queues = new QueueCollection(_logger, this, pipeline, _cancellation.Token );

            var senders = settings.KnownSubscribers.Where(x => x.Uri.Scheme == ProtocolName)
                .ToDictionary(x => x.Uri);

            foreach (var queue in settings.Loopback)
            {
                var receiver = _queues.AddQueue(queue.Name, queue.Parallelization);

                var uri = $"loopback://{queue.Name}".ToUri();

                senders.TryGetValue(uri, out SubscriberAddress subscriber);
                yield return new LoopbackChannel(subscriber ?? new SubscriberAddress(uri), receiver);
            }

            foreach (var sender in senders.Values.Where(x => !_queues.Has(x.Uri.QueueName())))
            {
                var receiver = _queues.AddQueue(sender.Uri.QueueName(), 5);
                yield return new LoopbackChannel(sender, receiver);
            }
        }

        public Uri DefaultReplyUri()
        {
            return TransportConstants.RetryUri;
        }

        public bool Enabled { get; } = true;

        public void Dispose()
        {
            _cancellation.Cancel();
            _queues?.Dispose();
        }

        public static readonly Uri Delayed = "loopback://delayed".ToUri();
        public static readonly Uri Retries = "loopback://retries".ToUri();
        private TransportSettings _settings;

        IMessageCallback IQueueProvider.BuildCallback(Envelope envelope, QueueReceiver receiver)
        {
            return new LightweightCallback(_retryChannel.Value);
        }

        void IQueueProvider.StoreIncomingMessages(Envelope[] messages)
        {
            // nothing
        }

        void IQueueProvider.RemoveIncomingMessages(Envelope[] messages)
        {
            // nothing
        }
    }


}

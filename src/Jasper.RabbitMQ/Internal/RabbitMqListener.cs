using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Transports;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqListener : RabbitMqConnectionAgent, IListener
    {
        private readonly ILogger _logger;
        private readonly RabbitMqEndpoint _endpoint;
        private readonly RabbitMqTransport _transport;
        private IListeningWorkerQueue _callback;
        private WorkerQueueMessageConsumer _consumer;
        private readonly string _routingKey;
        private readonly RabbitMqSender _sender;
        private CancellationToken _cancellation = CancellationToken.None;

        public RabbitMqListener(ILogger logger,
            RabbitMqEndpoint endpoint, RabbitMqTransport transport) : base(transport.ListeningConnection)
        {
            _logger = logger;
            _endpoint = endpoint;
            _transport = transport;
            Address = endpoint.Uri;

            _routingKey = endpoint.RoutingKey ?? endpoint.QueueName ?? "";

            _sender = new RabbitMqSender(_endpoint, _transport);
        }

        public override void Dispose()
        {
            _callback.Dispose();
            base.Dispose();
            _sender.Dispose();
        }

        public ListeningStatus Status
        {
            get => _consumer != null ? ListeningStatus.Accepting : ListeningStatus.TooBusy;
            set
            {
                switch (value)
                {
                    case ListeningStatus.TooBusy when _consumer != null:
                        _consumer.Dispose();

                        Channel.BasicCancel(_consumer.ConsumerTags.FirstOrDefault());
                        _consumer = null;
                        break;
                    case ListeningStatus.Accepting when _consumer == null:
                        Start(_callback, _cancellation);
                        break;
                }
            }
        }

        public void Start(IListeningWorkerQueue callback, CancellationToken cancellation)
        {
            if (callback == null) return;

            _cancellation = cancellation;
            _cancellation.Register(teardownChannel);

            EnsureConnected();

            _callback = callback;
            _consumer = new WorkerQueueMessageConsumer(callback, _logger, this, _endpoint, Address, _sender, _cancellation);

            Channel.BasicConsume(_consumer, _routingKey);
        }

        public Task<bool> TryRequeue(Envelope envelope)
        {
            if (envelope is RabbitMqEnvelope e)
            {
                e.Listener.Requeue(e);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Uri Address { get; }
        public ValueTask CompleteAsync(Envelope envelope)
        {
            return RabbitMqChannelCallback.Instance.CompleteAsync(envelope);
        }

        public ValueTask DeferAsync(Envelope envelope)
        {
            return RabbitMqChannelCallback.Instance.DeferAsync(envelope);
        }

        public ValueTask Requeue(RabbitMqEnvelope envelope)
        {
            if (!envelope.Acked)
            {
                Channel.BasicNack(envelope.DeliveryTag, false, false);
            }
            return _sender.Send(envelope);
        }

        public void Complete(ulong deliveryTag)
        {
            Channel.BasicAck(deliveryTag, false);
        }
    }
}

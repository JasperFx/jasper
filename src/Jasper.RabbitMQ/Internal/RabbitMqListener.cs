using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Transports;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqListener : RabbitMqConnectionAgent, IListener
    {
        private readonly RabbitMqEndpoint _endpoint;
        private readonly ILogger _logger;
        private readonly string _routingKey;
        private readonly RabbitMqSender _sender;
        private IListeningWorkerQueue? _callback;
        private CancellationToken _cancellation = CancellationToken.None;
        private WorkerQueueMessageConsumer? _consumer;

        public RabbitMqListener(ILogger logger,
            RabbitMqEndpoint endpoint, RabbitMqTransport transport) : base(transport.ListeningConnection)
        {
            _logger = logger;
            _endpoint = endpoint;
            Address = endpoint.Uri;

            _routingKey = endpoint.RoutingKey ?? endpoint.QueueName ?? "";

            _sender = new RabbitMqSender(_endpoint, transport);
        }

        public override void Dispose()
        {
            _callback?.Dispose();
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
                        Start(_callback!, _cancellation);
                        break;
                }
            }
        }

        public void Start(IListeningWorkerQueue callback, CancellationToken cancellation)
        {
            _cancellation = cancellation;
            _cancellation.Register(teardownChannel);

            EnsureConnected();

            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _consumer = new WorkerQueueMessageConsumer(callback, _logger, this, _endpoint, Address,
                _cancellation);

            Channel.BasicQos(_endpoint.PreFetchSize, _endpoint.PreFetchCount, false);

            Channel.BasicConsume(_consumer, _routingKey);
        }

        public async Task<bool> TryRequeueAsync(Envelope envelope)
        {
            if (envelope is not RabbitMqEnvelope e)
            {
                return false;
            }

            await e.Listener.RequeueAsync(e);
            return true;
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

        public ValueTask RequeueAsync(RabbitMqEnvelope envelope)
        {
            if (!envelope.Acknowledged)
            {
                Channel.BasicNack(envelope.DeliveryTag, false, false);
            }

            return _sender.SendAsync(envelope);
        }

        public void Complete(ulong deliveryTag)
        {
            Channel.BasicAck(deliveryTag, false);
        }
    }
}

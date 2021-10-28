using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqListener : RabbitMqConnectionAgent, IListener
    {
        private readonly ITransportLogger _logger;
        private readonly RabbitMqEndpoint _endpoint;
        private readonly RabbitMqTransport _transport;
        private readonly IRabbitMqProtocol _mapper;
        private IListeningWorkerQueue _callback;
        private WorkerQueueMessageConsumer _consumer;
        private readonly string _routingKey;
        private readonly RabbitMqSender _sender;

        public RabbitMqListener(ITransportLogger logger,
            RabbitMqEndpoint endpoint, RabbitMqTransport transport) : base(transport)
        {
            _logger = logger;
            _endpoint = endpoint;
            _transport = transport;
            _mapper = endpoint.Protocol;
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

                        Channel.BasicCancel(_consumer.ConsumerTag);
                        _consumer = null;
                        break;
                    case ListeningStatus.Accepting when _consumer == null:
                        Start(_callback);
                        break;
                }
            }
        }

        public void Start(IListeningWorkerQueue callback)
        {
            if (callback == null) return;

            EnsureConnected();

            _callback = callback;
            _consumer = new WorkerQueueMessageConsumer(callback, _logger, this, _mapper, Address, _sender)
            {
                ConsumerTag = Guid.NewGuid().ToString()
            };

            Channel.BasicConsume(_consumer, _routingKey);
        }

        public Uri Address { get; }
        public Task Complete(Envelope envelope)
        {
            return RabbitMqChannelCallback.Instance.Complete(envelope);
        }

        public Task Defer(Envelope envelope)
        {
            return RabbitMqChannelCallback.Instance.Defer(envelope);
        }

        public Task Requeue(RabbitMqEnvelope envelope)
        {
            // TODO -- watch if this needs to be requeued w/ the sender
            Channel.BasicNack(envelope.DeliveryTag, false, false);
            return _sender.Send(envelope);
        }

        public void Complete(ulong deliveryTag)
        {
            Channel.BasicAck(deliveryTag, false);
        }
    }
}

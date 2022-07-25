using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Transports;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqListener : RabbitMqConnectionAgent, IListener
    {
        private readonly ILogger _logger;
        private readonly string _routingKey;
        private readonly RabbitMqSender _sender;
        private IReceiver _receiver;
        private CancellationToken _cancellation = CancellationToken.None;
        private WorkerQueueMessageConsumer? _consumer;

        public RabbitMqListener(ILogger logger,
            RabbitMqEndpoint endpoint, RabbitMqTransport transport, IReceiver receiver) : base(transport.ListeningConnection, transport, endpoint, logger)
        {
            _logger = logger;
            Endpoint = endpoint;
            Address = endpoint.Uri;

            _routingKey = endpoint.RoutingKey ?? endpoint.QueueName ?? "";

            _sender = new RabbitMqSender(Endpoint, transport, RoutingMode.Static, logger);

            Start(receiver, _cancellation);
        }

        public RabbitMqEndpoint Endpoint { get; }

        public override void Dispose()
        {
            _receiver?.Dispose();
            base.Dispose();
            _sender.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask StopAsync()
        {
            Status = ListeningStatus.Stopped;
            if (_consumer == null) return ValueTask.CompletedTask;
            _consumer.Dispose();

            Channel.BasicCancel(_consumer.ConsumerTags.FirstOrDefault());
            _consumer = null;

            return ValueTask.CompletedTask;
        }

        public ValueTask RestartAsync()
        {
            Start(_receiver!, _cancellation);
            return ValueTask.CompletedTask;
        }

        public ListeningStatus Status
        {
            get;
            private set;
        }

        [Obsolete]
        public void Start(IReceiver callback, CancellationToken cancellation)
        {
            _cancellation = cancellation;
            _cancellation.Register(teardownChannel);

            EnsureConnected();

            _receiver = callback ?? throw new ArgumentNullException(nameof(callback));
            _consumer = new WorkerQueueMessageConsumer(callback, _logger, this, Endpoint, Address,
                _cancellation);

            Channel.BasicQos(Endpoint.PreFetchSize, Endpoint.PreFetchCount, false);

            Channel.BasicConsume(_consumer, _routingKey);
            Status = ListeningStatus.Accepting;
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

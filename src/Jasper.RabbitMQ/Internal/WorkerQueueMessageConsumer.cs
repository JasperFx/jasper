using System;
using System.Threading;
using Jasper.Logging;
using Jasper.Transports;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class WorkerQueueMessageConsumer : DefaultBasicConsumer, IDisposable
    {
        private readonly Uri _address;
        private readonly RabbitMqSender _rabbitMqSender;
        private readonly CancellationToken _cancellation;
        private readonly RabbitMqListener Listener;
        private readonly IListeningWorkerQueue _workerQueue;
        private readonly ILogger _logger;
        private readonly RabbitMqEndpoint _endpoint;
        private bool _latched;

        public WorkerQueueMessageConsumer(IListeningWorkerQueue workerQueue, ILogger logger,
            RabbitMqListener listener,
            RabbitMqEndpoint endpoint, Uri address, RabbitMqSender rabbitMqSender, CancellationToken cancellation)
        {
            _workerQueue = workerQueue;
            _logger = logger;
            Listener = listener;
            _endpoint = endpoint;
            _address = address;
            _rabbitMqSender = rabbitMqSender;
            _cancellation = cancellation;
        }

        public void Dispose()
        {
            _latched = true;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey,
            IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            if (_latched || _cancellation.IsCancellationRequested)
            {
                Listener.Channel.BasicReject(deliveryTag, true);
                return;
            }

            var envelope = new RabbitMqEnvelope(Listener, deliveryTag);
            try
            {
                envelope.Data = body.ToArray(); // TODO -- use byte sequence instead!
                _endpoint.MapIncomingToEnvelope(envelope, properties);
            }
            catch (Exception e)
            {
                _logger.LogError(e, message: "Error trying to map an incoming RabbitMQ message to an Envelope");
                Model.BasicAck(envelope.DeliveryTag, false);

                return;
            }

            if (envelope.IsPing())
            {
                Model.BasicAck(deliveryTag, false);
                return;
            }

            _workerQueue.Received(_address, envelope).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Failure to receive an incoming message with {Id}", envelope.Id);
                    Model.BasicNack(deliveryTag, false, true);
                }
            }, _cancellation);
        }
    }


}

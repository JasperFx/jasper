using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqSender : ISender
    {
        private readonly PublicationAddress _address;
        private readonly RabbitMqEndpoint _endpoint;
        private readonly CancellationToken _cancellation;
        private readonly ITransportLogger _logger;
        private readonly IRabbitMqProtocol _protocol;
        private ISenderCallback _callback;
        private ActionBlock<Envelope> _sending;
        private ActionBlock<Envelope> _serialization;

        public RabbitMqSender(ITransportLogger logger, RabbitMqEndpoint endpoint,
            CancellationToken cancellation)
        {
            throw new NotImplementedException();
//            _protocol = endpoint.Protocol;
//            _logger = logger;
//            _endpoint = endpoint;
//            _cancellation = cancellation;
//            Destination = endpoint.TransportUri.ToUri();
//
//            _address = new PublicationAddress(endpoint.ExchangeType.ToString(), endpoint.ExchangeName ?? "", endpoint.TransportUri.QueueName);
        }

        public void Dispose()
        {
            // Nothing, assuming that the agent owns the channel
        }

        public void Start(ISenderCallback callback)
        {
            throw new NotImplementedException();
//            _endpoint.Connect();
//
//            _callback = callback;
//
//            _serialization = new ActionBlock<Envelope>(e =>
//            {
//                try
//                {
//                    e.EnsureData();
//                    _sending.Post(e);
//                }
//                catch (Exception exception)
//                {
//                    _logger.LogException(exception, e.Id, "Serialization Failure!");
//                }
//            }, new ExecutionDataflowBlockOptions{CancellationToken = _cancellation});
//
//            _sending = new ActionBlock<Envelope>(send, new ExecutionDataflowBlockOptions
//            {
//                CancellationToken = _cancellation
//            });
        }

        public Task Enqueue(Envelope envelope)
        {
            _serialization.Post(envelope);

            return Task.CompletedTask;
        }

        public Uri Destination { get; }
        public int QueuedCount => _sending.InputCount;

        public bool Latched { get; private set; }

        public Task LatchAndDrain()
        {
            throw new NotImplementedException();
//            Latched = true;
//
//            _endpoint.Stop();
//
//            _sending.Complete();
//            _serialization.Complete();
//
//
//            _logger.CircuitBroken(Destination);
//
//            return Task.CompletedTask;
        }

        public void Unlatch()
        {
            _logger.CircuitResumed(Destination);

            Start(_callback);
            Latched = false;
        }

        public Task Ping()
        {
            throw new NotImplementedException();
//            return _endpoint.Ping(channel =>
//            {
//                var envelope = Envelope.ForPing(Destination);
//
//                var props = _endpoint.Channel.CreateBasicProperties();
//
//                _protocol.WriteFromEnvelope(envelope, props);
//                props.AppId = "Jasper";
//
//                channel.BasicPublish(_address, props, envelope.Data);
//            });
        }

        public bool SupportsNativeScheduledSend { get; } = false;

        private async Task send(Envelope envelope)
        {
            throw new NotImplementedException();
//            if (_endpoint.State == AgentState.Disconnected)
//                throw new InvalidOperationException($"The RabbitMQ agent for {_address} is disconnected");
//
//            try
//            {
//                var props = _endpoint.Channel.CreateBasicProperties();
//                props.Persistent = _endpoint.TransportUri.Durable;
//
//                _protocol.WriteFromEnvelope(envelope, props);
//
//                _endpoint.Channel.BasicPublish(_address, props, envelope.Data);
//
//                await _callback.Successful(envelope);
//            }
//            catch (Exception e)
//            {
//                try
//                {
//                    await _callback.ProcessingFailure(envelope, e);
//                }
//                catch (Exception exception)
//                {
//                    _logger.LogException(exception);
//                }
//            }
        }
    }
}

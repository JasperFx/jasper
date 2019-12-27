using System;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Logging;
using Jasper.Transports;
using Jasper.Transports.Sending;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqSender : RabbitMqConnectionAgent, ISender
    {
        private readonly CancellationToken _cancellation;
        private readonly ITransportLogger _logger;
        private readonly IRabbitMqProtocol _protocol;
        private ISenderCallback _callback;
        private ActionBlock<Envelope> _sending;
        private readonly string _exchangeName;
        private readonly string _key;
        private readonly bool _isDurable;

        public RabbitMqSender(ITransportLogger logger, RabbitMqEndpoint endpoint, RabbitMqTransport transport,
            CancellationToken cancellation) : base(transport)
        {
            _protocol = endpoint.Protocol;
            _logger = logger;
            _cancellation = cancellation;
            Destination = endpoint.Uri;

            _isDurable = endpoint.IsDurable;

            _exchangeName = endpoint.ExchangeName == TransportConstants.Default ? "" : endpoint.ExchangeName;
            _key = endpoint.RoutingKey ?? endpoint.QueueName ?? "";
        }



        public void Start(ISenderCallback callback)
        {
            Connect();

            _callback = callback;

            _sending = new ActionBlock<Envelope>(send, new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cancellation
            });
        }

        public Task Enqueue(Envelope envelope)
        {
            _sending.Post(envelope);

            return Task.CompletedTask;
        }

        public Uri Destination { get; }
        public int QueuedCount => _sending.InputCount;

        public bool Latched { get; private set; }

        public Task LatchAndDrain()
        {
            Latched = true;

            Stop();

            _sending.Complete();


            _logger.CircuitBroken(Destination);

            return Task.CompletedTask;
        }



        public void Unlatch()
        {
            _logger.CircuitResumed(Destination);

            Start(_callback);
            Latched = false;
        }

        public Task Ping()
        {
            return Ping(channel =>
            {
                var envelope = Envelope.ForPing(Destination);

                var props = Channel.CreateBasicProperties();

                _protocol.WriteFromEnvelope(envelope, props);
                props.AppId = "Jasper";

                channel.BasicPublish(_exchangeName, _key, props, envelope.Data);
            });
        }

        internal Task Ping(Action<IModel> action)
        {
            lock (_locker)
            {
                if (State == AgentState.Connected) return Task.CompletedTask;

                startNewConnection();


                try
                {
                    action(Channel);
                }
                catch (Exception)
                {
                    teardownConnection();
                    throw;
                }
            }


            return Task.CompletedTask;
        }

        public bool SupportsNativeScheduledSend { get; } = false;

        private async Task send(Envelope envelope)
        {
            if (State == AgentState.Disconnected)
                throw new InvalidOperationException($"The RabbitMQ agent for {Destination} is disconnected");

            try
            {
                var props = Channel.CreateBasicProperties();
                props.Persistent = _isDurable;

                _protocol.WriteFromEnvelope(envelope, props);

                Channel.BasicPublish(_exchangeName, _key, props, envelope.Data);

                await _callback.Successful(envelope);
            }
            catch (Exception e)
            {
                try
                {
                    await _callback.ProcessingFailure(envelope, e);
                }
                catch (Exception exception)
                {
                    _logger.LogException(exception);
                }
            }
        }
    }
}

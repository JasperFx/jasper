using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Transports;
using Jasper.Transports.Sending;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqSender : RabbitMqConnectionAgent, ISender
    {
        private readonly IRabbitMqProtocol _protocol;
        private readonly string _exchangeName;
        private readonly string _key;
        private readonly bool _isDurable;
        public bool SupportsNativeScheduledSend { get; } = false;
        public Uri Destination { get; }

        public RabbitMqSender(RabbitMqEndpoint endpoint, RabbitMqTransport transport) : base(transport)
        {
            _protocol = endpoint.Protocol;
            Destination = endpoint.Uri;
            
            _isDurable = endpoint.IsDurable;

            _exchangeName = endpoint.ExchangeName == TransportConstants.Default ? "" : endpoint.ExchangeName;
            _key = endpoint.RoutingKey ?? endpoint.QueueName ?? "";
        }

#pragma warning disable 1998
        public async Task Send(Envelope envelope)
#pragma warning restore 1998
        {
            EnsureConnected();

            if (State == AgentState.Disconnected)
                throw new InvalidOperationException($"The RabbitMQ agent for {Destination} is disconnected");

            var props = Channel.CreateBasicProperties();
            props.Persistent = _isDurable;

            _protocol.WriteFromEnvelope(envelope, props);

            Channel.BasicPublish(_exchangeName, _key, props, envelope.Data);
        }

        public Task<bool> Ping(CancellationToken cancellationToken)
        {
            lock (_locker)
            {
                if (State == AgentState.Connected) return Task.FromResult(true);

                startNewConnection();

                if (Channel.IsOpen)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    teardownConnection();
                    return Task.FromResult(false);
                }
            }
        }
    }
}

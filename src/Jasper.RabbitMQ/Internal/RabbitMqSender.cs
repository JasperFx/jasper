using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Transports;
using Jasper.Transports.Sending;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqSender : RabbitMqConnectionAgent, ISender
    {
        private readonly RabbitMqEndpoint _endpoint;
        private readonly string _exchangeName;
        private readonly string _key;
        private readonly bool _isDurable;
        public bool SupportsNativeScheduledSend { get; } = false;
        public Uri Destination { get; }

        public RabbitMqSender(RabbitMqEndpoint endpoint, RabbitMqTransport transport) : base(transport.SendingConnection)
        {
            _endpoint = endpoint;
            Destination = endpoint.Uri;

            _isDurable = endpoint.Mode == EndpointMode.Durable;

            _exchangeName = endpoint.ExchangeName == TransportConstants.Default ? "" : endpoint.ExchangeName;
            _key = endpoint.RoutingKey ?? endpoint.QueueName ?? "";
        }

        public ValueTask Send(Envelope envelope)
        {
            EnsureConnected();

            if (State == AgentState.Disconnected)
                throw new InvalidOperationException($"The RabbitMQ agent for {Destination} is disconnected");

            var props = Channel.CreateBasicProperties();
            props.Persistent = _isDurable;
            props.Headers = new Dictionary<string, object>();

            _endpoint.MapEnvelopeToOutgoing(envelope, props);

            Channel.BasicPublish(_exchangeName, _key, props, envelope.Data);

            return ValueTask.CompletedTask;
        }

        public Task<bool> Ping(CancellationToken cancellationToken)
        {
            lock (_locker)
            {
                if (State == AgentState.Connected) return Task.FromResult(true);

                startNewChannel();

                if (Channel.IsOpen)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    teardownChannel();
                    return Task.FromResult(false);
                }
            }
        }
    }
}

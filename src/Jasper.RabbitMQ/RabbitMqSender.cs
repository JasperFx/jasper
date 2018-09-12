using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public class RabbitMqSender : ISender
    {
        private readonly PublicationAddress _address;
        private readonly RabbitMqAgent _agent;
        private readonly CancellationToken _cancellation;
        private readonly ITransportLogger _logger;
        private readonly IEnvelopeMapper _mapper;
        private ISenderCallback _callback;
        private ActionBlock<Envelope> _sending;
        private ActionBlock<Envelope> _serialization;

        public RabbitMqSender(ITransportLogger logger, RabbitMqAgent agent,
            CancellationToken cancellation)
        {
            _mapper = agent.EnvelopeMapping;
            _logger = logger;
            _agent = agent;
            _cancellation = cancellation;
            Destination = agent.Uri;

            _address = agent.PublicationAddress();
        }

        public void Dispose()
        {
            // Nothing, assuming that the agent owns the channel
        }

        public void Start(ISenderCallback callback)
        {
            _agent.Start();

            _callback = callback;

            _serialization = new ActionBlock<Envelope>(e =>
            {
                try
                {
                    e.EnsureData();
                    _sending.Post(e);
                }
                catch (Exception exception)
                {
                    _logger.LogException(exception, e.Id, "Serialization Failure!");
                }
            });

            _sending = new ActionBlock<Envelope>(send, new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cancellation
            });
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
            Latched = true;

            _agent.Stop();

            _sending.Complete();
            _serialization.Complete();


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
            return _agent.Ping(channel =>
            {
                var envelope = Envelope.ForPing();
                envelope.Destination = Destination;

                var props = _agent.Channel.CreateBasicProperties();

                _mapper.WriteFromEnvelope(envelope, props);
                props.AppId = "Jasper";

                channel.BasicPublish(_address, props, envelope.Data);
            });
        }

        private async Task send(Envelope envelope)
        {
            if (_agent.State == AgentState.Disconnected)
                throw new InvalidOperationException($"The RabbitMQ agent for {_address} is disconnected");

            try
            {
                var props = _agent.Channel.CreateBasicProperties();
                props.Persistent = _agent.IsDurable;

                _mapper.WriteFromEnvelope(envelope, props);
                _agent.Channel.BasicPublish(_address, props, envelope.Data);

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

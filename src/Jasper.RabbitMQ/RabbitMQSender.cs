using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Messaging;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.Transports.Tcp;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public class RabbitMQSender : ISender
    {
        private readonly IEnvelopeMapper _mapper;
        private readonly ITransportLogger _logger;
        private readonly RabbitMqAgent _agent;
        private readonly IModel _channel;
        private readonly CancellationToken _cancellation;
        private ActionBlock<Envelope> _block;
        private readonly PublicationAddress _address;
        private ISenderCallback _callback;

        public RabbitMQSender(ITransportLogger logger, RabbitMqAgent agent, IModel channel, CancellationToken cancellation)
        {
            _mapper = agent.EnvelopeMapping;
            _logger = logger;
            _agent = agent;
            _channel = channel;
            _cancellation = cancellation;
            Destination = agent.Uri;

            _address = agent.PublicationAddress();
        }

        private Task send(Envelope envelope)
        {
            // TODO -- get rid of this w/ single envelope version of ISenderCallback methods
            var batch = new OutgoingMessageBatch(Destination, new Envelope[] {envelope});
            try
            {
                var props = _channel.CreateBasicProperties();
                props.Persistent = _agent.IsDurable;

                _mapper.WriteFromEnvelope(envelope, props);
                _channel.BasicPublish(_address, props, envelope.Data);

                return _callback.Successful(batch);
            }
            catch (Exception e)
            {
                return _callback.ProcessingFailure(batch, e);
            }
        }

        public void Dispose()
        {
            // Nothing, assuming that the agent owns the channel
        }

        public void Start(ISenderCallback callback)
        {
            _callback = callback;

            // TODO -- add a second block for serialization?
            _block = new ActionBlock<Envelope>(send, new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cancellation,
            });
        }

        public Task Enqueue(Envelope envelope)
        {
            try
            {
                envelope.EnsureData();
                _block.Post(envelope);
            }
            catch (Exception e)
            {
                _logger.LogException(e, envelope.Id, "Message serialization failure in outgoing RabbitMQ message!");

            }

            return Task.CompletedTask;
        }

        public Uri Destination { get; }
        public int QueuedCount => _block.InputCount;

        public bool Latched { get; private set; }
        public Task LatchAndDrain()
        {
            Latched = true;

            _block.Complete();


            _logger.CircuitBroken(Destination);


            return _block.Completion;
        }

        public void Unlatch()
        {
            _logger.CircuitResumed(Destination);

            Start(_callback);
            Latched = false;
        }

        public Task Ping()
        {
            var envelope = Envelope.ForPing();
            envelope.Destination = Destination;

            var props = _channel.CreateBasicProperties();

            _mapper.WriteFromEnvelope(envelope, props);
            _channel.BasicPublish(_address, props, envelope.Data);

            return Task.CompletedTask;
        }
    }
}

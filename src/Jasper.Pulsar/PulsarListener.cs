using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Jasper.Transports;

namespace Jasper.Pulsar
{
    internal class PulsarListener : IListener
    {
        private readonly PulsarEndpoint _endpoint;
        private readonly PulsarTransport _transport;
        private IConsumer<ReadOnlySequence<byte>> _consumer;
        private Task _receivingLoop;
        private CancellationToken _cancellation;
        private readonly PulsarSender _sender;
        private IListeningWorkerQueue _callback;

        public PulsarListener(PulsarEndpoint endpoint, PulsarTransport transport, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _transport = transport;
            _cancellation = cancellation;

            Address = endpoint.Uri;

            _sender = new PulsarSender(endpoint, transport, _cancellation);
        }

        public Task Complete(Envelope envelope)
        {
            if (envelope is PulsarEnvelope e)
            {
                return _consumer.Acknowledge(e.Message, _cancellation).AsTask();
            }

            return Task.CompletedTask;
        }

        public async Task Defer(Envelope envelope)
        {
            if (envelope is PulsarEnvelope e)
            {
                await _consumer.Acknowledge(e.Message, _cancellation);
                await _sender.Send(envelope);
            }
        }

        public void Dispose()
        {
            // TODO -- no mixing!
            _consumer.DisposeAsync().GetAwaiter().GetResult();
            _sender.Dispose();
            _receivingLoop.Dispose();
        }

        public Uri Address { get; }

        // TODO -- make the transitions happen with methods
        public ListeningStatus Status
        {
            get => _consumer != null ? ListeningStatus.Accepting : ListeningStatus.TooBusy;
            set
            {
                switch (value)
                {
                    case ListeningStatus.TooBusy when _consumer != null:
                        _consumer?.DisposeAsync();

                        _consumer = null;
                        break;
                    case ListeningStatus.Accepting when _consumer == null:
                        Start(_callback, _cancellation);
                        break;
                }
            }
        }
        public void Start(IListeningWorkerQueue callback, CancellationToken cancellation)
        {
            _cancellation = cancellation;
            _callback = callback;

            _consumer = _transport.Client.NewConsumer()
                .SubscriptionName("Jasper")
                // TODO -- more options here. Give the user complete
                // control over the Pulsar usage. Maybe expose ConsumerOptions on endpoint
                .Topic(_endpoint.PulsarTopic())
                .Create();

            _receivingLoop = Task.Run(async () =>
            {
                await foreach (var message in _consumer.Messages(cancellationToken: cancellation))
                {
                    Debug.WriteLine(message.MessageId);
                    var envelope = new PulsarEnvelope(message);

                    // TODO -- invoke the deserialization here. A
                    envelope.Data = message.Data.ToArray();
                    _endpoint.MapIncomingToEnvelope(envelope, message);

                    // TODO -- the worker queue should already have the Uri,
                    // so just take in envelope
                    await callback.Received(Address, envelope);
                }
            }, cancellation);
        }

        public async Task<bool> TryRequeue(Envelope envelope)
        {
            if (envelope is PulsarEnvelope e)
            {
                await _sender.Send(envelope);
                return true;
            }

            return false;
        }
    }
}

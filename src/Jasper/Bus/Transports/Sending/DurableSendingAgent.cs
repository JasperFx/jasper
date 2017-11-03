using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Sending
{
    public class DurableSendingAgent : ISendingAgent, ISenderCallback
    {
        private readonly IPersistence _persistence;
        private readonly ISender _sender;
        private readonly CancellationToken _cancellation;

        public DurableSendingAgent(Uri destination, IPersistence persistence, ISender sender, CancellationToken cancellation)
        {
            _persistence = persistence;
            _sender = sender;
            _cancellation = cancellation;
            Destination = destination;
        }

        public Uri Destination { get; }
        public Uri DefaultReplyUri { get; set; }

        public Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.EnsureData();

            envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;

            _sender.Enqueue(envelope);
            return Task.CompletedTask;
        }

        public Task StoreAndForward(Envelope envelope)
        {
            envelope.EnsureData();

            // TODO -- that'll be async later
            _persistence.StoreOutgoing(envelope);

            return EnqueueOutgoing(envelope);
        }

        public void Start()
        {
            _sender.Start(this);
        }

        public void Successful(OutgoingMessageBatch outgoing)
        {
            _persistence.RemoveOutgoing(outgoing.Messages);
        }

        void ISenderCallback.TimedOut(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.SerializationFailure(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception)
        {
            processRetry(outgoing);
        }

        private void processRetry(OutgoingMessageBatch outgoing)
        {
            // TODO -- all of this is temporary
            int maximumAttempts = 3;

            foreach (var message in outgoing.Messages)
            {
                message.SentAttempts++;
            }

            var groups = outgoing
                .Messages
                .Where(x => x.SentAttempts < maximumAttempts)
                .GroupBy(x => x.SentAttempts);

            foreach (var group in groups)
            {
                var delayTime = (group.Key * group.Key).Seconds();
                var messages = group.ToArray();

                Task.Delay(delayTime, _cancellation).ContinueWith(_ =>
                {
                    if (_cancellation.IsCancellationRequested)
                    {
                        return;
                    }

                    foreach (var message in messages)
                    {
                        _sender.Enqueue(message);
                    }
                }, _cancellation);
            }

            _persistence.PersistBasedOnSentAttempts(outgoing, maximumAttempts);
        }

        public void Dispose()
        {
            _sender?.Dispose();
        }
    }
}

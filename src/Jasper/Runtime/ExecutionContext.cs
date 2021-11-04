using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Persistence.Durability;
using Jasper.Transports;
using Jasper.Util;

namespace Jasper.Runtime
{
    public class ExecutionContext : MessagePublisher, IExecutionContext, IEnvelopeTransaction
    {
        private object _sagaId;
        private IChannelCallback _channel;
        private readonly IList<Envelope> _scheduled = new List<Envelope>();

        public ExecutionContext(IMessagingRoot root) : base(root, CombGuidIdGeneration.NewGuid())
        {
        }

        internal void ClearState()
        {
            _outstanding.Clear();
            _scheduled.Clear();
            Envelope = null;
            Transaction = null;
            _sagaId = null;
        }

        internal void ReadEnvelope(Envelope originalEnvelope, IChannelCallback channel)
        {
            Envelope = originalEnvelope ?? throw new ArgumentNullException(nameof(originalEnvelope));
            CorrelationId = originalEnvelope.CorrelationId;
            _channel = channel;
            _sagaId = originalEnvelope.SagaId;

            EnlistInTransaction(this);

            if (Envelope.AckRequested)
            {
                var ack = Root.Acknowledgements.BuildAcknowledgement(Envelope);

                _outstanding.Add(ack);
            }
        }

        Task IEnvelopeTransaction.Persist(Envelope envelope)
        {
            _outstanding.Fill(envelope);
            return Task.CompletedTask;
        }

        Task IEnvelopeTransaction.Persist(Envelope[] envelopes)
        {
            _outstanding.Fill(envelopes);
            return Task.CompletedTask;
        }

        Task IEnvelopeTransaction.ScheduleJob(Envelope envelope)
        {
            _scheduled.Fill(envelope);
            return Task.CompletedTask;
        }

        async Task IEnvelopeTransaction.CopyTo(IEnvelopeTransaction other)
        {
            await other.Persist(_outstanding.ToArray());

            foreach (var envelope in _scheduled)
            {
                await other.ScheduleJob(envelope);
            }
        }

        /// <summary>
        ///     Send a response message back to the original sender of the message being handled.
        ///     This can only be used from within a message handler
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public Task RespondToSender(object response)
        {
            if (Envelope == null)
            {
                throw new InvalidOperationException(
                    "This operation can only be performed while in the middle of handling an incoming message");
            }

            if (Envelope.ReplyUri == null)
            {
                throw new ArgumentOutOfRangeException(nameof(Envelope), $"There is no {nameof(Envelope.ReplyUri)}");
            }

            return SendToDestination(Envelope.ReplyUri, response);
        }


        public async Task EnqueueCascading(object message)
        {
            if (Envelope.ResponseType != null && (message?.GetType() == Envelope.ResponseType ||
                                                  Envelope.ResponseType.IsAssignableFrom(message?.GetType())))
            {
                Envelope.Response = message;
                return;
            }

            switch (message)
            {
                case null:
                    return;

                case Envelope env:
                    await SendEnvelope(env);
                    return;

                case IEnumerable<object> enumerable:
                    foreach (var o in enumerable) await EnqueueCascading(o);

                    return;
            }

            if (message.GetType().ToMessageTypeName() == Envelope.ReplyRequested)
            {
                await SendToDestination(Envelope.ReplyUri, message);
                return;
            }


            await Publish(message);
        }


        IMessagePublisher IExecutionContext.NewPublisher()
        {
            return Root.NewContext();
        }

        public Envelope Envelope { get; protected set; }


        public async Task SendAllQueuedOutgoingMessages()
        {
            foreach (var envelope in Outstanding)
            {
                try
                {
                    await envelope.QuickSend();
                }
                catch (Exception e)
                {
                    Logger.LogException(e, envelope.CorrelationId,
                        "Unable to send an outgoing message, most likely due to serialization issues");
                    Logger.DiscardedEnvelope(envelope);
                }
            }

            if (ReferenceEquals(Transaction, this))
            {
                await flushScheduledMessages();
            }

            _outstanding.Clear();
        }

        private async Task flushScheduledMessages()
        {
            if (Persistence is NulloEnvelopePersistence)
            {
                foreach (var envelope in _scheduled)
                {
                    Root.ScheduledJobs.Enqueue(envelope.ExecutionTime.Value, envelope);
                }
            }
            else
            {
                foreach (var envelope in _scheduled)
                {
                    await Persistence.ScheduleJob(envelope);
                }
            }

            _scheduled.Clear();
        }

        public void UseInMemoryTransaction()
        {
            if (!ReferenceEquals(this, Transaction))
            {
                EnlistInTransaction(this);
            }
        }

        public void EnlistInSaga(object sagaId)
        {
            _sagaId = sagaId ?? throw new ArgumentNullException(nameof(sagaId));
            foreach (var envelope in _outstanding) envelope.SagaId = sagaId.ToString();
        }

        Envelope IAcknowledgementSender.BuildAcknowledgement(Envelope envelope)
        {
            return Root.Acknowledgements.BuildAcknowledgement(envelope);
        }

        Task IAcknowledgementSender.SendAcknowledgement(Envelope envelope)
        {
            return Root.Acknowledgements.SendAcknowledgement(envelope);
        }

        Task IAcknowledgementSender.SendFailureAcknowledgement(Envelope original, string message)
        {
            return Root.Acknowledgements.SendFailureAcknowledgement(original, message);
        }

        public Task Complete()
        {
            return _channel.Complete(Envelope);
        }

        public Task Defer()
        {
            return _channel.Defer(Envelope);
        }

        public async Task ReSchedule(DateTime scheduledTime)
        {
            Envelope.ExecutionTime = scheduledTime;
            if (_channel is IHasNativeScheduling c)
            {
                await c.MoveToScheduledUntil(Envelope, Envelope.ExecutionTime.Value);
            }
            else
            {
                await Persistence.ScheduleJob(Envelope);
            }
        }

        public async Task MoveToDeadLetterQueue(Exception exception)
        {
            if (_channel is IHasDeadLetterQueue c)
            {
                await c.MoveToErrors(Envelope, exception);
            }
            else
            {
                // If persistable, persist
                await Persistence.MoveToDeadLetterStorage(Envelope, exception);
            }
        }

        public Task RetryExecutionNow()
        {
            return Root.Pipeline.Invoke(Envelope, _channel);
        }

        protected override void trackEnvelopeCorrelation(Envelope outbound)
        {
            base.trackEnvelopeCorrelation(outbound);
            outbound.SagaId = _sagaId?.ToString() ?? Envelope?.SagaId ?? outbound.SagaId;

            if (Envelope != null)
            {
                outbound.CausationId = Envelope.Id;
            }
        }
    }
}

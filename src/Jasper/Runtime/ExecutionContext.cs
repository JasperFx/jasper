using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Persistence.Durability;
using Jasper.Transports;
using Jasper.Util;

namespace Jasper.Runtime
{
    public class ExecutionContext : MessagePublisher, IExecutionContext
    {
        private object _sagaId;


        public ExecutionContext(IMessagingRoot root) : base(root, CombGuidIdGeneration.NewGuid())
        {
        }

        public ExecutionContext(IMessagingRoot root, Envelope originalEnvelope, IChannelCallback channel) : base(root,
            originalEnvelope.CorrelationId)
        {
            Envelope = originalEnvelope ?? throw new ArgumentNullException(nameof(originalEnvelope));
            Channel = channel;
            _sagaId = originalEnvelope.SagaId;

            var transaction = new InMemoryEnvelopeTransaction();
            EnlistInTransaction(transaction);

            if (Envelope.AckRequested)
            {
                var ack = Root.Acknowledgements.BuildAcknowledgement(Envelope);

                transaction.Queued.Fill(ack);
                _outstanding.Add(ack);
            }
        }

        public IChannelCallback Channel { get; }

        /// <summary>
        ///     Send a response message back to the original sender of the message being handled.
        ///     This can only be used from within a message handler
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
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

        public Envelope Envelope { get; }


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

            _outstanding.Clear();
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

        Task IExecutionContext.Complete()
        {
            return Channel.Complete(Envelope);
        }

        Task IExecutionContext.Defer()
        {
            return Channel.Defer(Envelope);
        }

        async Task IExecutionContext.ReSchedule(DateTime scheduledTime)
        {
            Envelope.ExecutionTime = scheduledTime;
            if (Channel is IHasNativeScheduling c)
            {
                await c.MoveToScheduledUntil(Envelope, Envelope.ExecutionTime.Value);
            }
            else
            {
                await Persistence.ScheduleJob(Envelope);
            }
        }

        async Task IExecutionContext.MoveToDeadLetterQueue(Exception exception)
        {
            if (Channel is IHasDeadLetterQueue c)
            {
                await c.MoveToErrors(Envelope, exception);
            }
            else
            {
                // If persistable, persist
                await Persistence.MoveToDeadLetterStorage(Envelope, exception);
            }
        }

        Task IExecutionContext.RetryExecutionNow()
        {
            return Root.Pipeline.Invoke(Envelope, Channel);
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

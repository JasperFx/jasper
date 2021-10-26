using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Transports;
using Jasper.Util;
using Lamar;

namespace Jasper.Runtime
{
    public class CommandBus : ICommandBus
    {
        // TODO -- smelly that this is protected, stop that!
        protected readonly List<Envelope> _outstanding = new List<Envelope>();

        [DefaultConstructor]
        public CommandBus(IMessagingRoot root) : this(root, CombGuidIdGeneration.NewGuid())
        {
        }

        public CommandBus(IMessagingRoot root, Guid correlationId)
        {
            Root = root;
            Persistence = root.Persistence;
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }

        public IMessagingRoot Root { get; }
        public IEnvelopePersistence Persistence { get; }


        public IMessageLogger Logger => Root.MessageLogger;

        public IEnumerable<Envelope> Outstanding => _outstanding;


        public Task Invoke(object message)
        {
            return Root.Pipeline.InvokeNow(new Envelope(message)
            {
                ReplyUri = TransportConstants.RepliesUri,
                CorrelationId = CorrelationId
            });
        }

        public async Task<T> Invoke<T>(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var envelope = new Envelope(message)
            {
                ReplyUri = TransportConstants.RepliesUri,
                ReplyRequested = typeof(T).ToMessageTypeName(),
                ResponseType = typeof(T),
                CorrelationId = CorrelationId
            };

            await Root.Pipeline.InvokeNow(envelope);

            if (envelope.Response == null)
            {
                return default(T);
            }

            return (T)envelope.Response;
        }

        public Task Enqueue<T>(T message)
        {
            var envelope = Root.Router.RouteLocally(message);
            return persistOrSend(envelope);
        }

        public Task Enqueue<T>(T message, string workerQueue)
        {
            var envelope = Root.Router.RouteLocally(message, workerQueue);

            return persistOrSend(envelope);
        }

        public async Task<Guid> Schedule<T>(T message, DateTimeOffset executionTime)
        {
            var envelope = new Envelope(message)
            {
                ExecutionTime = executionTime,
                Destination = TransportConstants.DurableLocalUri
            };

            var writer = Root.Serialization.JsonWriterFor(message.GetType());
            envelope.Data = writer.Write(message);
            envelope.ContentType = writer.ContentType;

            envelope.Status = EnvelopeStatus.Scheduled;
            envelope.OwnerId = TransportConstants.AnyNode;

            await ScheduleEnvelope(envelope);

            return envelope.Id;
        }

        public Task<Guid> Schedule<T>(T message, TimeSpan delay)
        {
            return Schedule(message, DateTimeOffset.UtcNow.Add(delay));
        }

        internal Task ScheduleEnvelope(Envelope envelope)
        {
            if (envelope.Message == null)
                throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Message is required");

            if (!envelope.ExecutionTime.HasValue)
                throw new ArgumentOutOfRangeException(nameof(envelope), "No value for ExecutionTime");


            envelope.OwnerId = TransportConstants.AnyNode;
            envelope.Status = EnvelopeStatus.Scheduled;

            if (EnlistedInTransaction)
            {
                return Transaction.ScheduleJob(envelope);
            }

            if (Persistence is NulloEnvelopePersistence)
            {
                Root.ScheduledJobs.Enqueue(envelope.ExecutionTime.Value, envelope);
                return Task.CompletedTask;
            }

            return Persistence.ScheduleJob(envelope);
        }

        private Task persistOrSend(Envelope envelope)
        {
            if (EnlistedInTransaction)
            {
                _outstanding.Add(envelope);
                return envelope.Sender.IsDurable ? Transaction.Persist(envelope) : Task.CompletedTask;
            }

            return envelope.Send();
        }

        public bool EnlistedInTransaction { get; protected set; }

        public Task EnlistInTransaction(IEnvelopeTransaction transaction)
        {
            var original = Transaction;
            Transaction = transaction;
            EnlistedInTransaction = true;

            return original?.CopyTo(transaction) ?? Task.CompletedTask;
        }

        public IEnvelopeTransaction Transaction { get; private set; }
    }
}

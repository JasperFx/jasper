using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
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
        public CommandBus(IJasperRuntime runtime) : this(runtime, Guid.NewGuid().ToString())
        {
        }

        public CommandBus(IJasperRuntime runtime, string? correlationId)
        {
            Runtime = runtime;
            Persistence = runtime.Persistence;
            CorrelationId = correlationId;
        }

        public string? CorrelationId { get; protected set; }

        public IJasperRuntime Runtime { get; }
        public IEnvelopePersistence Persistence { get; }


        public IMessageLogger Logger => Runtime.MessageLogger;

        public IEnumerable<Envelope> Outstanding => _outstanding;


        public Task InvokeAsync(object message, CancellationToken cancellation = default)
        {
            return Runtime.Pipeline.InvokeNow(new Envelope(message)
            {
                ReplyUri = TransportConstants.RepliesUri,
                CorrelationId = CorrelationId
            }, cancellation);
        }

        public async Task<T?> InvokeAsync<T>(object message, CancellationToken cancellation = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var envelope = new Envelope(message)
            {
                ReplyUri = TransportConstants.RepliesUri,
                ReplyRequested = typeof(T).ToMessageTypeName(),
                ResponseType = typeof(T),
                CorrelationId = CorrelationId
            };

            await Runtime.Pipeline.InvokeNow(envelope, cancellation);

            if (envelope.Response == null)
            {
                return default;
            }

            return (T)envelope.Response;
        }

        public Task Enqueue<T>(T message)
        {
            var envelope = Runtime.Router.RouteLocally(message);
            return persistOrSend(envelope);
        }

        public Task Enqueue<T>(T message, string workerQueueName)
        {
            var envelope = Runtime.Router.RouteLocally(message, workerQueueName);

            return persistOrSend(envelope);
        }

        public async Task<Guid> Schedule<T>(T message, DateTimeOffset executionTime)
        {
            var envelope = new Envelope(message)
            {
                ScheduledTime = executionTime,
                Destination = TransportConstants.DurableLocalUri
            };

            var endpoint = Runtime.Endpoints.For(TransportConstants.DurableLocalUri);

            var writer = endpoint.DefaultSerializer;
            envelope.Data = writer.Write(message);
            envelope.ContentType = writer.ContentType;

            envelope.Status = EnvelopeStatus.Scheduled;
            envelope.OwnerId = TransportConstants.AnyNode;

            await ScheduleEnvelope(envelope);

            return envelope.Id;
        }

        public Task<Guid> Schedule<T>(T? message, TimeSpan delay)
        {
            return Schedule(message, DateTimeOffset.UtcNow.Add(delay));
        }

        internal Task ScheduleEnvelope(Envelope envelope)
        {
            if (envelope.Message == null)
                throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Message is required");

            if (!envelope.ScheduledTime.HasValue)
                throw new ArgumentOutOfRangeException(nameof(envelope), "No value for ExecutionTime");


            envelope.OwnerId = TransportConstants.AnyNode;
            envelope.Status = EnvelopeStatus.Scheduled;

            if (EnlistedInTransaction)
            {
                return Transaction.ScheduleJobAsync(envelope);
            }

            if (Persistence is NulloEnvelopePersistence)
            {
                Runtime.ScheduledJobs.Enqueue(envelope.ScheduledTime.Value, envelope);
                return Task.CompletedTask;
            }

            return Persistence.ScheduleJobAsync(envelope);
        }

        private Task persistOrSend(Envelope? envelope)
        {
            if (EnlistedInTransaction)
            {
                _outstanding.Fill(envelope);
                return envelope.Sender.IsDurable ? Transaction.PersistAsync(envelope) : Task.CompletedTask;
            }

            return envelope.StoreAndForward();
        }

        public bool EnlistedInTransaction { get; protected set; }

        public Task EnlistInTransaction(IEnvelopeTransaction transaction)
        {
            var original = Transaction;
            Transaction = transaction;
            EnlistedInTransaction = true;

            return original?.CopyToAsync(transaction) ?? Task.CompletedTask;
        }

        public IEnvelopeTransaction Transaction { get; protected set; }
    }
}

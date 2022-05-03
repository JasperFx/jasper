using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten.Persistence.Operations;
using Jasper.Persistence.Postgresql;
using Marten;
using Marten.Services;

namespace Jasper.Persistence.Marten
{
    public class MartenEnvelopeTransaction : IEnvelopeTransaction
    {
        private readonly int _nodeId;
        private readonly IDocumentSession _session;
        private readonly PostgresqlSettings _settings;

        public MartenEnvelopeTransaction(IDocumentSession session, IExecutionContext bus)
        {
            if (bus.Persistence is PostgresqlEnvelopePersistence persistence)
            {
                _settings = (PostgresqlSettings) persistence.DatabaseSettings;
                _nodeId = persistence.Settings.UniqueNodeId;
            }
            else
            {
                throw new InvalidOperationException(
                    "This Jasper application is not using Postgresql + Marten as the backing message persistence");
            }

            _session = session;

        }

        public Task PersistAsync(Envelope envelope)
        {
            _session.StoreOutgoing(_settings, envelope, _nodeId);
            return Task.CompletedTask;
        }

        public Task PersistAsync(Envelope[] envelopes)
        {
            foreach (var envelope in envelopes) _session.StoreOutgoing(_settings, envelope, _nodeId);

            return Task.CompletedTask;
        }

        public Task ScheduleJobAsync(Envelope envelope)
        {
            _session.StoreIncoming(_settings, envelope);
            return Task.CompletedTask;
        }

        public Task CopyToAsync(IEnvelopeTransaction other)
        {
            throw new NotSupportedException();
        }
    }

    public class FlushOutgoingMessagesOnCommit : DocumentSessionListenerBase
    {
        private readonly IExecutionContext _bus;

        public FlushOutgoingMessagesOnCommit(IExecutionContext bus)
        {
            _bus = bus;
        }

        public override void AfterCommit(IDocumentSession session, IChangeSet commit)
        {
            _bus.SendAllQueuedOutgoingMessages().GetAwaiter().GetResult();
        }

        public override Task AfterCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token)
        {
            return _bus.SendAllQueuedOutgoingMessages();
        }
    }
}

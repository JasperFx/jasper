using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Persistence.Marten.Persistence;
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

        public MartenEnvelopeTransaction(IDocumentSession session, IMessageContext bus)
        {
            if (bus.Advanced.Persistence is PostgresqlEnvelopePersistence persistence)
            {
                _settings = (PostgresqlSettings) persistence.Settings;
                _nodeId = persistence.Options.UniqueNodeId;
            }
            else
            {
                throw new InvalidOperationException(
                    "This Jasper application is not using Postgresql + Marten as the backing message persistence");
            }

            _session = session;

        }

        public Task Persist(Envelope envelope)
        {
            _session.StoreOutgoing(_settings, envelope, _nodeId);
            return Task.CompletedTask;
        }

        public Task Persist(Envelope[] envelopes)
        {
            foreach (var envelope in envelopes) _session.StoreOutgoing(_settings, envelope, _nodeId);

            return Task.CompletedTask;
        }

        public Task ScheduleJob(Envelope envelope)
        {
            _session.StoreIncoming(_settings, envelope);
            return Task.CompletedTask;
        }

        public Task CopyTo(IEnvelopeTransaction other)
        {
            throw new NotSupportedException();
        }
    }

    public class FlushOutgoingMessagesOnCommit : DocumentSessionListenerBase
    {
        private readonly IMessageContext _bus;

        public FlushOutgoingMessagesOnCommit(IMessageContext bus)
        {
            _bus = bus;
        }

        public override void AfterCommit(IDocumentSession session, IChangeSet commit)
        {
            _bus.SendAllQueuedOutgoingMessages();
        }

        public override Task AfterCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token)
        {
            return _bus.SendAllQueuedOutgoingMessages();
        }
    }
}

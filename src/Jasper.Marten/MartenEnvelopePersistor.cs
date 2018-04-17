using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Marten.Persistence;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Marten;
using Marten.Services;

namespace Jasper.Marten
{
    public class MartenEnvelopePersistor : IEnvelopePersistor
    {
        private readonly IDocumentSession _session;
        private readonly int _nodeId;
        private readonly EnvelopeTables _tables;

        public MartenEnvelopePersistor(IDocumentSession session, IMessageContext bus)
        {
            if (!(bus.Advanced.Persistence is MartenBackedMessagePersistence))
            {
                throw new InvalidOperationException("This Jasper application is not using Marten as the backing message persistence");
            }

            var martenPersistence = bus.Advanced.Persistence.As<MartenBackedMessagePersistence>();

            _nodeId = martenPersistence.Settings.UniqueNodeId;
            _tables = martenPersistence.Tables;
            _session = session;


        }

        public Task Persist(Envelope envelope)
        {
            _session.StoreOutgoing(_tables, envelope, _nodeId);
            return Task.CompletedTask;
        }

        public Task Persist(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                _session.StoreOutgoing(_tables, envelope, _nodeId);
            }

            return Task.CompletedTask;
        }

        public Task ScheduleJob(Envelope envelope)
        {
            _session.StoreIncoming(_tables, envelope);
            return Task.CompletedTask;
        }

        public Task CopyTo(IEnvelopePersistor other)
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

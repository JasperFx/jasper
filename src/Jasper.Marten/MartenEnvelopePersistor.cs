using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Marten.Persistence;
using Jasper.Marten.Persistence.Resiliency;
using Marten;
using Marten.Services;

namespace Jasper.Marten
{
    public class MartenEnvelopePersistor : IEnvelopePersistor
    {
        private readonly IDocumentSession _session;
        private readonly int _nodeId;
        private readonly EnvelopeTables _tables;

        public MartenEnvelopePersistor(IDocumentSession session, IServiceBus bus)
        {
            if (!(bus.Persistence is MartenBackedMessagePersistence))
            {
                throw new InvalidOperationException("This Jasper application is not using Marten as the backing message persistence");
            }

            var martenPersistence = bus.Persistence.As<MartenBackedMessagePersistence>();

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
    }

    public class FlushOutgoingMessagesOnCommit : DocumentSessionListenerBase
    {
        private readonly IServiceBus _bus;

        public FlushOutgoingMessagesOnCommit(IServiceBus bus)
        {
            _bus = bus;
        }

        public override void AfterCommit(IDocumentSession session, IChangeSet commit)
        {
            _bus.FlushOutstanding();
        }

        public override Task AfterCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token)
        {
            return _bus.FlushOutstanding();
        }
    }

    public static class ServiceBusExtensions
    {
        public static void EnlistInTransaction(this IServiceBus bus, IDocumentSession session)
        {
            var persistor = new MartenEnvelopePersistor(session, bus);
            session.Listeners.Add(new FlushOutgoingMessagesOnCommit(bus));

            bus.EnlistInTransaction(persistor);
        }
    }


}

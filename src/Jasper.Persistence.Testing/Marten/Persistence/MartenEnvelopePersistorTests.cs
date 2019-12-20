using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Postgresql;
using Marten;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class MartenEnvelopePersistorTests : PostgresqlContext, IDisposable
    {
        public MartenEnvelopePersistorTests()
        {
            var store = theHost.Get<IDocumentStore>();
            store.Advanced.Clean.CompletelyRemoveAll();
            theHost.RebuildMessageStorage();
        }

        public void Dispose()
        {
            theHost?.Dispose();
        }

        public IHost theHost = JasperHost.For<ItemReceiver>();

        [Fact]
        public async Task get_counts()
        {
            var thePersistor = theHost.Get<PostgresqlEnvelopePersistence>();

            var list = new List<Envelope>();

            // 10 incoming
            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Incoming;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());


            // 7 scheduled
            list.Clear();
            for (var i = 0; i < 7; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Scheduled;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());


            // 3 outgoing
            list.Clear();
            for (var i = 0; i < 3; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Outgoing;

                list.Add(envelope);
            }

            await thePersistor.StoreOutgoing(list.ToArray(), 0);

            var counts = await thePersistor.Admin.GetPersistedCounts();

            counts.Incoming.ShouldBe(10);
            counts.Scheduled.ShouldBe(7);
            counts.Outgoing.ShouldBe(3);
        }
    }
}

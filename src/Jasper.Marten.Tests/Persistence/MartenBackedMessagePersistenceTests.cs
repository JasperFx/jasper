using System;
using System.Linq;
using Baseline.Dates;
using Jasper.Marten.Persistence;
using Jasper.Marten.Persistence.DbObjects;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Tests.Setup;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Testing.Messaging;
using Jasper.Testing.Messaging.Runtime;
using Marten;
using Xunit;
using Shouldly;

namespace Jasper.Marten.Tests.Persistence
{
    public class MartenBackedMessagePersistenceTests : IDisposable
    {
        private JasperRuntime theRuntime;
        private Envelope theEnvelope;
        private Envelope persisted;

        public MartenBackedMessagePersistenceTests()
        {
            theRuntime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(ConnectionSource.ConnectionString);
                _.ConfigureMarten(x =>
                {
                    x.Storage.Add<PostgresqlEnvelopeStorage>();
                    x.PLV8Enabled = false;
                });

            });

            theRuntime.Get<IDocumentStore>().Schema.ApplyAllConfiguredChangesToDatabase();

            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new Message1();
            theEnvelope.ExecutionTime = DateTime.Today.ToUniversalTime().AddDays(1);

            theRuntime.Get<MartenBackedMessagePersistence>().ScheduleJob(theEnvelope).Wait(3.Seconds());



            using (var session = theRuntime.Get<IDocumentStore>().LightweightSession())
            {
                persisted = session.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);
            }
        }

        [Fact]
        public void should_persist_the_scheduled_envelope()
        {
            persisted.ShouldNotBeNull();
        }

        [Fact]
        public void should_be_in_scheduled_status()
        {
            persisted.Status.ShouldBe(TransportConstants.Scheduled);
        }

        [Fact]
        public void should_be_owned_by_any_node()
        {
            persisted.OwnerId.ShouldBe(TransportConstants.AnyNode);
        }

        public void Dispose()
        {
            theRuntime.Dispose();
        }
    }
}

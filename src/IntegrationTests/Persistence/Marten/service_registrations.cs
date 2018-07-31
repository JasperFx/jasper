using System;
using Jasper;
using Marten;
using Servers;
using Servers.Docker;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.Marten
{
    public class service_registrations : MartenContext
    {
        [Fact]
        public void registers_document_store_in_a_usable_way()
        {
            using (var runtime = JasperRuntime.For<MartenUsingApp>())
            {
                var doc = new FakeDoc {Id = Guid.NewGuid()};


                using (var session = runtime.Get<IDocumentSession>())
                {
                    session.Store(doc);
                    session.SaveChanges();
                }

                using (var query = runtime.Get<IQuerySession>())
                {
                    query.Load<FakeDoc>(doc.Id).ShouldNotBeNull();
                }
            }
        }

        public service_registrations(DockerFixture<MartenContainer> fixture) : base(fixture)
        {
        }
    }

    public class FakeDoc
    {
        public Guid Id { get; set; }
    }

    public class MartenUsingApp : JasperRegistry
    {
        public MartenUsingApp()
        {
            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(MartenContainer.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            });
        }
    }
}

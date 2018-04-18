using System;
using Jasper.Marten.Tests.Setup;
using Jasper.Testing.Binding;
using Marten;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests
{
    public class service_registrations
    {
        [Fact]
        public void registers_document_store_in_a_usable_way()
        {
            using (var runtime = JasperRuntime.For<MartenUsingApp>())
            {
                var doc = new FakeDoc{Id = Guid.NewGuid()};


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
                _.Connection(ConnectionSource.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            });
        }
    }
}

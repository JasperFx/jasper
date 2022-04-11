using System;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Marten;
using Shouldly;
using Weasel.Core;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class service_registrations : PostgresqlContext
    {
        [Fact]
        public void registers_document_store_in_a_usable_way()
        {
            using (var runtime = JasperHost.For(opts =>
                   {
                       opts.Extensions.UseMarten(o =>
                       {
                           o.Connection(Servers.PostgresConnectionString);
                           o.AutoCreateSchemaObjects = AutoCreate.All;
                       });
                   }))
            {
                var doc = new FakeDoc { Id = Guid.NewGuid() };


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
}

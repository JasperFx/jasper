using System;
using System.Linq;
using IntegrationTests;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Postgresql;
using Lamar;
using Marten;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Weasel.Core;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class service_registrations : PostgresqlContext
    {
        [Fact]
        public void basic_registrations()
        {
            using var host = Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.Services.AddMarten(o =>
                    {
                        o.Connection(Servers.PostgresConnectionString);
                        o.AutoCreateSchemaObjects = AutoCreate.All;
                    }).IntegrateWithJasper();
                }).Start();

            var container = (IContainer)host.Services;

            container.Model.For<IEnvelopePersistence>()
                .Default.ImplementationType.ShouldBe(typeof(PostgresqlEnvelopePersistence));

            container.Model.For<IJasperExtension>().Instances
                .Any(x => x.ImplementationType == typeof(MartenMiddlewareExtension))
                .ShouldBeTrue();

            container.GetInstance<PostgresqlSettings>()
                .SchemaName.ShouldBe("public");
        }

        [Fact]
        public void override_schema_name()
        {
            using var host = Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.Services.AddMarten(o =>
                    {
                        o.Connection(Servers.PostgresConnectionString);
                        o.AutoCreateSchemaObjects = AutoCreate.All;
                    }).IntegrateWithJasper("jasper");
                }).Start();

            var container = (IContainer)host.Services;

            container.Model.For<IEnvelopePersistence>()
                .Default.ImplementationType.ShouldBe(typeof(PostgresqlEnvelopePersistence));

            container.Model.For<IJasperExtension>().Instances
                .Any(x => x.ImplementationType == typeof(MartenMiddlewareExtension))
                .ShouldBeTrue();

            container.GetInstance<PostgresqlSettings>()
                .SchemaName.ShouldBe("jasper");
        }

        [Fact]
        public void registers_document_store_in_a_usable_way()
        {
            using var runtime = JasperHost.For(opts =>
            {
                opts.Services.AddMarten(o =>
                {
                    o.Connection(Servers.PostgresConnectionString);
                    o.AutoCreateSchemaObjects = AutoCreate.All;
                }).IntegrateWithJasper();
            });

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

    public class FakeDoc
    {
        public Guid Id { get; set; }
    }
}

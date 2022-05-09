using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Jasper.Persistence.SqlServer;
using Jasper.Tcp;
using Jasper.Tracking;
using Marten;
using Microsoft.Extensions.Hosting;
using Oakton.Resources;
using Shouldly;
using TestingSupport;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class end_to_end_with_persistence : PostgresqlContext, IDisposable, IAsyncLifetime
    {
        private readonly ITestOutputHelper _output;

        public end_to_end_with_persistence(ITestOutputHelper output)
        {
            _output = output;
            theSender = JasperHost.For(opts =>
            {
                opts.Publish(x =>
                {
                    x.Message<ItemCreated>();
                    x.Message<Question>();
                    x.ToPort(2345).Durably();
                });

                opts.Services.AddMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.DatabaseSchemaName = "sender";
                }).IntegrateWithJasper();

                opts.ListenAtPort(2567);
            });

            theReceiver = JasperHost.For(opts =>
            {
                opts.Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "receiver");

                opts.ListenAtPort(2345).DurablyPersistedLocally();

                opts.Services.AddMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.DatabaseSchemaName = "receiver";
                }).IntegrateWithJasper();
            });
        }

        public async Task InitializeAsync()
        {
            await theSender.ResetResourceState();
            await theReceiver.ResetResourceState();
        }

        public Task DisposeAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        private readonly IHost theSender;
        private readonly IHost theReceiver;


        [Fact]
        public async Task delete_all_persisted_envelopes()
        {
            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };


            await theSender.Get<IMessagePublisher>().ScheduleAsync(item, 1.Days());

            var persistor = theSender.Get<IEnvelopePersistence>();

            var counts = await persistor.Admin.FetchCountsAsync();

            counts.Scheduled.ShouldBe(1);

            await persistor.Admin.ClearAllAsync();

            (await persistor.Admin.FetchCountsAsync()).Scheduled.ShouldBe(0);
        }

        [Fact]
        public async Task enqueue_locally()
        {
            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            await theReceiver.ExecuteAndWaitValueTaskAsync(c => c.EnqueueAsync(item));


            var documentStore = theReceiver.Get<IDocumentStore>();
            using (var session = documentStore.QuerySession())
            {
                var item2 = session.Load<ItemCreated>(item.Id);
                if (item2 == null)
                {
                    Thread.Sleep(500);
                    item2 = session.Load<ItemCreated>(item.Id);
                }


                item2.Name.ShouldBe("Shoe");
            }

            var incoming = await theReceiver.Get<IEnvelopePersistence>().Admin.AllIncomingAsync();
            incoming.Any().ShouldBeFalse();
        }

    }
}

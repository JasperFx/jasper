using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Persistence.Durability;
using Jasper.Tracking;
using Marten;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class end_to_end_with_persistence : PostgresqlContext, IDisposable
    {
        private readonly ITestOutputHelper _output;

        public end_to_end_with_persistence(ITestOutputHelper output)
        {
            _output = output;
            theSender = JasperHost.For<ItemSender>();
            theReceiver = JasperHost.For<ItemReceiver>();

            theSender.RebuildMessageStorage().GetAwaiter().GetResult();
            theReceiver.RebuildMessageStorage().GetAwaiter().GetResult();

        }

        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        private readonly IHost theSender;
        private readonly IHost theReceiver;


        [Fact]
        public void can_get_storage_sql()
        {
            var sql = theSender.Get<IEnvelopePersistence>().Admin.CreateSql();

            sql.ShouldNotBeNull();

            _output.WriteLine(sql);
        }

        [Fact]
        public async Task delete_all_persisted_envelopes()
        {
            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };


            await theSender.Get<IMessagePublisher>().Schedule(item, 1.Days());

            var persistor = theSender.Get<IEnvelopePersistence>();

            var counts = await persistor.Admin.GetPersistedCounts();

            counts.Scheduled.ShouldBe(1);

            await persistor.Admin.ClearAllPersistedEnvelopes();

            (await persistor.Admin.GetPersistedCounts()).Scheduled.ShouldBe(0);
        }

        [Fact]
        public async Task enqueue_locally()
        {
            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            await theReceiver.ExecuteAndWait(c => c.Enqueue(item));


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

            var incoming = await theReceiver.Get<IEnvelopePersistence>().Admin.AllIncomingEnvelopes();
            incoming.Any().ShouldBeFalse();
        }


        [Fact] // TODO -- this needs a Retry of some sort
        public async Task send_end_to_end()
        {
            var item = new ItemCreated
            {
                Name = "Hat",
                Id = Guid.NewGuid()
            };

            await theSender
                .TrackActivity()
                .AlsoTrack(theReceiver)
                // In case there are trash, leftover persisted messages
                .DoNotAssertOnExceptionsDetected()
                .SendMessageAndWait(item);


            await using (var session = theReceiver.Get<IDocumentStore>().QuerySession())
            {
                var item2 = session.Load<ItemCreated>(item.Id);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (item2 == null && stopwatch.ElapsedMilliseconds < 15000)
                {
                    await Task.Delay(500);
                    item2 = session.Load<ItemCreated>(item.Id);
                }

                stopwatch.Stop();

                item2.Name.ShouldBe("Hat");

            }

            var admin = theReceiver.Get<IEnvelopePersistence>().Admin;
            (await admin.AllIncomingEnvelopes()).Any().ShouldBeFalse();
        }
    }
}

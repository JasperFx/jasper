using System;
using System.Linq;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.EntityFrameworkCore;
using Jasper.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Persistence.Testing.EFCore
{
    public class EFCorePersistenceContext : BaseContext
    {
        public EFCorePersistenceContext() : base(false)
        {
            builder.ConfigureServices((c, services) =>
            {
                services.AddDbContext<SampleDbContext>(x => x.UseSqlServer(Servers.SqlServerConnectionString));
            })
            .UseJasper(registry =>
            {
                registry.Services.AddSingleton<PassRecorder>();
                registry.Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);
            });
        }
    }

    public class end_to_end_efcore_persistence : IClassFixture<EFCorePersistenceContext>
    {
        private readonly EFCorePersistenceContext _context;

        public end_to_end_efcore_persistence(EFCorePersistenceContext context)
        {
            Host = context.theHost;
            Host.RebuildMessageStorage();
        }

        public IHost Host { get; }

        [Fact]
        public async Task persist_an_outgoing_envelope()
        {
            var envelope = new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                OwnerId = 5,
                Destination = TransportConstants.RetryUri,
                DeliverBy = new DateTimeOffset(DateTime.Today),
            };

            var context = Host.Services.GetRequiredService<SampleDbContext>();
            var messaging = Host.Services.GetRequiredService<IMessageContext>();

            var transaction = new EFCoreEnvelopeTransaction(context, messaging);

            await transaction.Persist(envelope);
            await context.SaveChangesAsync();

            var persisted = await Host.Services.GetRequiredService<IEnvelopePersistence>()
                .Admin.AllOutgoingEnvelopes();

            var loadedEnvelope = persisted.Single();

            loadedEnvelope.Id.ShouldBe(envelope.Id);

            loadedEnvelope.Destination.ShouldBe(envelope.Destination);
            loadedEnvelope.DeliverBy.ShouldBe(envelope.DeliverBy);
            loadedEnvelope.Data.ShouldBe(envelope.Data);



            loadedEnvelope.OwnerId.ShouldBe(envelope.OwnerId);
        }

        [Fact]
        public async Task persist_an_incoming_envelope()
        {
            var envelope = new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                OwnerId = 5,
                ExecutionTime = DateTime.Today.AddDays(1),
                DeliverBy = new DateTimeOffset(DateTime.Today),
                Status = TransportConstants.Scheduled,
                Attempts = 2
            };

            var context = Host.Services.GetRequiredService<SampleDbContext>();
            var messaging = Host.Services.GetRequiredService<IMessageContext>();

            var transaction = new EFCoreEnvelopeTransaction(context, messaging);

            await transaction.ScheduleJob(envelope);
            await context.SaveChangesAsync();

            var persisted = await Host.Services.GetRequiredService<IEnvelopePersistence>()
                .Admin.AllIncomingEnvelopes();

            var loadedEnvelope = persisted.Single();

            loadedEnvelope.Id.ShouldBe(envelope.Id);

            loadedEnvelope.Destination.ShouldBe(envelope.Destination);
            loadedEnvelope.ExecutionTime.ShouldBe(envelope.ExecutionTime);
            loadedEnvelope.Data.ShouldBe(envelope.Data);
            loadedEnvelope.OwnerId.ShouldBe(envelope.OwnerId);
            loadedEnvelope.Attempts.ShouldBe(envelope.Attempts);
        }
    }

    public class PassRecorder
    {
        private readonly TaskCompletionSource<Pass> _completion = new TaskCompletionSource<Pass>();

        public void Record(Pass pass)
        {
            _completion.SetResult(pass);
        }

        public Task<Pass> Actual => _completion.Task;
    }

    public class PassHandler
    {
        private readonly PassRecorder _recorder;

        public PassHandler(PassRecorder recorder)
        {
            _recorder = recorder;
        }

        public void Handle(Pass pass)
        {
            _recorder.Record(pass);
        }
    }

    public class Pass
    {
        public string From { get; set; }
        public string To { get; set; }
    }
}

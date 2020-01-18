using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.Postgresql.Schema;
using Jasper.TestSupport.Storyteller.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StoryTeller;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
using Host = Microsoft.AspNetCore.WebHost;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace StorytellerSpecs.Fixtures.Marten
{
    public class MartenScheduledJobFixture : Fixture
    {
        private IHost theHost;
        private ScheduledMessageReceiver theReceiver;

        public MartenScheduledJobFixture()
        {
            Title = "Marten Scheduled Jobs";
        }

        public override void SetUp()
        {
            var admin = new PostgresqlEnvelopeStorageAdmin(new PostgresqlSettings
                {ConnectionString = Servers.PostgresConnectionString});
            admin.RecreateAll();

            var registry = new ScheduledMessageApp();
            theReceiver = registry.Receiver;

            var logger = new StorytellerAspNetCoreLogger();
            Context.Reporting.Log(logger);


            theHost = Host
                .CreateDefaultBuilder()
                .ConfigureLogging(x => x.AddProvider(logger))
                .UseJasper(registry)
                .Start();
        }

        public override void TearDown()
        {
            theHost?.Dispose();
        }

        [FormatAs("Schedule message locally {id} for {seconds} seconds from now")]
        public Task ScheduleMessage(int id, int seconds)
        {
            return theHost.Services.GetService<IMessageContext>()
                .Schedule(new ScheduledMessage {Id = id}, seconds.Seconds());
        }

        [FormatAs("Schedule send message {id} for {seconds} seconds from now")]
        public Task ScheduleSendMessage(int id, int seconds)
        {
            return theHost.Services.GetService<IMessageContext>()
                .ScheduleSend(new ScheduledMessage {Id = id}, seconds.Seconds());
        }

        [FormatAs("The received message count should be {count}")]
        public int ReceivedMessageCount()
        {
            return theReceiver.ReceivedMessages.Count;
        }

        [FormatAs("Wait for at least one message to be received")]
        public Task AfterReceivingMessages()
        {
            return theReceiver.Received;
        }

        [FormatAs("The id of the only received message should be {id}")]
        public int TheIdOfTheOnlyReceivedMessageShouldBe()
        {
            return theReceiver.ReceivedMessages.Single().Id;
        }

        [FormatAs("The persisted count of scheduled jobs should be {count}")]
        public async Task<int> PersistedScheduledCount()
        {
            var counts = await theHost.Services.GetService<IEnvelopePersistence>().Admin.GetPersistedCounts();
            return counts.Scheduled;
        }
    }

    public class ScheduledMessageApp : JasperOptions
    {
        public readonly ScheduledMessageReceiver Receiver = new ScheduledMessageReceiver();

        public ScheduledMessageApp()
        {
            Services.AddSingleton(Receiver);

            Endpoints.Publish(x => x.MessagesFromAssemblyContaining<ScheduledMessageApp>()
                .ToLocalQueue("incoming").Durable());

            Handlers.Discovery(x =>
            {
                x.DisableConventionalDiscovery();
                x.IncludeType<ScheduledMessageCatcher>();
            });

            Extensions.UseMarten(Servers.PostgresConnectionString);
        }
    }

    public class ScheduledMessage
    {
        public int Id { get; set; }
    }


    public class ScheduledMessageReceiver
    {
        public readonly IList<ScheduledMessage> ReceivedMessages = new List<ScheduledMessage>();

        public readonly TaskCompletionSource<ScheduledMessage> Source = new TaskCompletionSource<ScheduledMessage>();

        public Task<ScheduledMessage> Received => Source.Task;
    }

    public class ScheduledMessageCatcher
    {
        private readonly ScheduledMessageReceiver _receiver;

        public ScheduledMessageCatcher(ScheduledMessageReceiver receiver)
        {
            _receiver = receiver;
        }


        public void Consume(ScheduledMessage message)
        {
            if (!_receiver.Source.Task.IsCompleted) _receiver.Source.SetResult(message);

            _receiver.ReceivedMessages.Add(message);
        }
    }
}

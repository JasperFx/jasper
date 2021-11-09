using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer.ScheduledJobs
{

    public class scheduled_jobs : IAsyncLifetime
    {
        private IHost theHost;
        private ScheduledMessageReceiver theReceiver;

        public async Task InitializeAsync()
        {
            var admin = new SqlServerEnvelopeStorageAdmin(new SqlServerSettings()
                {ConnectionString = Servers.SqlServerConnectionString});
            await admin.RecreateAll();

            var registry = new ScheduledMessageApp();
            theReceiver = registry.Receiver;


            theHost = await Host
                .CreateDefaultBuilder()
                .UseJasper(registry)
                .StartAsync();
        }

        public Task DisposeAsync()
        {
            return theHost.StopAsync();
        }

        public Task ScheduleSendMessage(int id, int seconds)
        {
            return theHost.Services.GetService<IExecutionContext>()
                .ScheduleSend(new ScheduledMessage {Id = id}, seconds.Seconds());
        }

        public int ReceivedMessageCount()
        {
            return theReceiver.ReceivedMessages.Count;
        }

        public Task AfterReceivingMessages()
        {
            return theReceiver.Received;
        }

        public int TheIdOfTheOnlyReceivedMessageShouldBe()
        {
            return theReceiver.ReceivedMessages.Single().Id;
        }

        public async Task<int> PersistedScheduledCount()
        {
            var counts = await theHost.Services.GetService<IEnvelopePersistence>().Admin.GetPersistedCounts();
            return counts.Scheduled;
        }

        [Fact]
        public async Task execute_scheduled_job()
        {
            await ScheduleSendMessage(1, 7200);
            await ScheduleSendMessage(2, 5);
            await ScheduleSendMessage(3, 7200);

            ReceivedMessageCount().ShouldBe(0);
            await AfterReceivingMessages();
            TheIdOfTheOnlyReceivedMessageShouldBe().ShouldBe(2);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed < 10.Seconds())
            {
                var count = await PersistedScheduledCount();
                if (count == 2) return;

                await Task.Delay(100.Milliseconds());
            }

            throw new Exception("The persisted count never reached 2");
        }
    }

    public class ScheduledMessageApp : JasperOptions
    {
        public readonly ScheduledMessageReceiver Receiver = new ScheduledMessageReceiver();

        public ScheduledMessageApp()
        {
            Services.AddSingleton(Receiver);

            Endpoints.Publish(x => x.MessagesFromAssemblyContaining<ScheduledMessageApp>()
                .ToLocalQueue("incoming").DurablyPersistedLocally());

            Handlers.Discovery(x =>
            {
                x.DisableConventionalDiscovery();
                x.IncludeType<ScheduledMessageCatcher>();
            });

            Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);
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

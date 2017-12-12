using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Marten.Persistence;
using Jasper.Marten.Tests.Setup;
using Marten;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Persistence
{
    public class delayed_jobs_backed_by_marten : IDisposable
    {
        private JasperRuntime theRuntime;

        public delayed_jobs_backed_by_marten()
        {
            DelayedMessageHandler.Reset();
            theRuntime = JasperRuntime.For<DelayedMessageApp>();

            theRuntime.Get<IDocumentStore>().Advanced.Clean.DeleteAllDocuments();
            theRuntime.Get<IDocumentStore>().Tenancy.Default.EnsureStorageExists(typeof(Envelope));
            theRuntime.Get<MartenBackedMessagePersistence>().ClearAllStoredMessages();


        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        [Fact]
        public async Task run_scheduled_job_locally()
        {
            var message1 = new DelayedMessage{Id = 1};
            var message2 = new DelayedMessage{Id = 2};
            var message3 = new DelayedMessage{Id = 3};


            await theRuntime.Bus.Schedule(message1, 2.Hours());

            var id = await theRuntime.Bus.Schedule(message2, 5.Seconds());

            await theRuntime.Bus.Schedule(message3, 2.Hours());

            DelayedMessageHandler.ReceivedMessages.Count.ShouldBe(0);




            DelayedMessageHandler.Received.Wait(10.Seconds());

            DelayedMessageHandler.ReceivedMessages.Single()
                .Id.ShouldBe(2);

        }

        [Fact]
        public async Task run_scheduled_job_after_being_sent()
        {
            var message1 = new DelayedMessage{Id = 1};
            var message2 = new DelayedMessage{Id = 22};
            var message3 = new DelayedMessage{Id = 3};

            using (var sender = JasperRuntime.For<SenderApp>())
            {
                await sender.Bus.DelaySend(message1, 2.Hours());
                await sender.Bus.DelaySend(message2, 5.Seconds());
                await sender.Bus.DelaySend(message3, 2.Hours());

                DelayedMessageHandler.ReceivedMessages.Count.ShouldBe(0);

                DelayedMessageHandler.Received.Wait(10.Seconds());

                DelayedMessageHandler.ReceivedMessages.Single()
                    .Id.ShouldBe(22);
            }






        }
    }

    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Settings.MartenConnectionStringIs(ConnectionSource.ConnectionString);

            Include<MartenBackedPersistence>();

            Logging.UseConsoleLogging = true;

            Publish.Message<DelayedMessage>().To("tcp://localhost:2777");
        }
    }

    public class DelayedMessageApp : JasperRegistry
    {
        public DelayedMessageApp() : base()
        {
            Transports.LightweightListenerAt(2777);

            Settings.MartenConnectionStringIs(ConnectionSource.ConnectionString);

            Include<MartenBackedPersistence>();

            Logging.UseConsoleLogging = true;
        }
    }

    public class DelayedMessage
    {
        public int Id { get; set; }
    }

    public class DelayedMessageHandler
    {
        public static readonly IList<DelayedMessage> ReceivedMessages = new List<DelayedMessage>();

        private static TaskCompletionSource<DelayedMessage> _source;

        public void Consume(DelayedMessage message)
        {
            _source?.TrySetResult(message);

            ReceivedMessages.Add(message);
        }

        public static void Reset()
        {
            _source = new TaskCompletionSource<DelayedMessage>();
            Received = _source.Task;
            ReceivedMessages.Clear();
        }

        public static Task<DelayedMessage> Received { get; private set; }
    }
}

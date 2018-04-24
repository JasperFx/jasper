using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Transports.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests.Persistence
{
    public class scheduled_jobs_backed_by_sqlserver : IDisposable
    {
        private JasperRuntime theRuntime;

        public scheduled_jobs_backed_by_sqlserver()
        {
            ScheduledMessageHandler.Reset();
            theRuntime = JasperRuntime.For<ScheduledMessageApp>();

            theRuntime.RebuildMessageStorage();
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        [Fact] // This thing is not super, duper reliable because of the timing. Needs tlove to watch
        // it in a smarter way
        public async Task run_scheduled_job_locally()
        {
            var message1 = new ScheduledMessage{Id = 1};
            var message2 = new ScheduledMessage{Id = 2};
            var message3 = new ScheduledMessage{Id = 3};


            await theRuntime.Messaging.Schedule(message1, 2.Hours());

            var id = await theRuntime.Messaging.Schedule(message2, 5.Seconds());

            await theRuntime.Messaging.Schedule(message3, 2.Hours());

            ScheduledMessageHandler.ReceivedMessages.Count.ShouldBe(0);




            ScheduledMessageHandler.Received.Wait(20.Seconds());

            ScheduledMessageHandler.ReceivedMessages.Single()
                .Id.ShouldBe(2);

        }

        [Fact]
        public async Task run_scheduled_job_after_being_sent()
        {
            var message1 = new ScheduledMessage{Id = 1};
            var message2 = new ScheduledMessage{Id = 22};
            var message3 = new ScheduledMessage{Id = 3};

            using (var sender = JasperRuntime.For<SenderApp>())
            {
                await sender.Messaging.ScheduleSend(message1, 2.Hours());
                await sender.Messaging.ScheduleSend(message2, 5.Seconds());
                await sender.Messaging.ScheduleSend(message3, 2.Hours());

                ScheduledMessageHandler.ReceivedMessages.Count.ShouldBe(0);

                ScheduledMessageHandler.Received.Wait(10.Seconds());

                ScheduledMessageHandler.ReceivedMessages.Single()
                    .Id.ShouldBe(22);
            }






        }
    }

    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Settings.PersistMessagesWithSqlServer(ConnectionSource.ConnectionString);

            Publish.Message<ScheduledMessage>().To("tcp://localhost:2777");
        }
    }

    public class ScheduledMessageApp : JasperRegistry
    {
        public ScheduledMessageApp() : base()
        {
            Transports.LightweightListenerAt(2777);

            Settings.PersistMessagesWithSqlServer(ConnectionSource.ConnectionString);
        }
    }

    public class ScheduledMessage
    {
        public int Id { get; set; }
    }

    public class ScheduledMessageHandler
    {
        public static readonly IList<ScheduledMessage> ReceivedMessages = new List<ScheduledMessage>();

        private static TaskCompletionSource<ScheduledMessage> _source;

        public void Consume(ScheduledMessage message)
        {
            _source?.TrySetResult(message);

            ReceivedMessages.Add(message);
        }

        public static void Reset()
        {
            _source = new TaskCompletionSource<ScheduledMessage>();
            Received = _source.Task;
            ReceivedMessages.Clear();
        }

        public static Task<ScheduledMessage> Received { get; private set; }
    }
}

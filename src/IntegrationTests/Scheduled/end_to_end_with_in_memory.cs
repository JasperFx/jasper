using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Shouldly;
using Xunit;

namespace IntegrationTests.Scheduled
{
    public class end_to_end_with_in_memory : IDisposable
    {
        private JasperRuntime theRuntime;

        public end_to_end_with_in_memory()
        {
            ScheduledMessageHandler.Reset();

            theRuntime = JasperRuntime.For<ScheduledMessageApp>();
        }

        [Fact]
        public void run_scheduled_job_locally()
        {
            var message1 = new ScheduledMessage{Id = 1};
            var message2 = new ScheduledMessage{Id = 2};
            var message3 = new ScheduledMessage{Id = 3};


            theRuntime.Bus.Schedule(message1, 2.Hours());
            theRuntime.Bus.Schedule(message2, 5.Seconds());

            theRuntime.Bus.Schedule(message3, 2.Hours());




            ScheduledMessageHandler.ReceivedMessages.Count.ShouldBe(0);

            ScheduledMessageHandler.Received.Wait(10.Seconds());

            ScheduledMessageHandler.ReceivedMessages.Single()
                .Id.ShouldBe(2);
        }

        [Fact]
        public void send_in_a_delayed_message()
        {
            var message1 = new ScheduledMessage{Id = 1};
            var message2 = new ScheduledMessage{Id = 2};
            var message3 = new ScheduledMessage{Id = 3};


            theRuntime.Bus.ScheduleSend(message1, 2.Hours());
            theRuntime.Bus.ScheduleSend(message2, 5.Seconds());

            theRuntime.Bus.ScheduleSend(message3, 2.Hours());




            ScheduledMessageHandler.ReceivedMessages.Count.ShouldBe(0);

            ScheduledMessageHandler.Received.Wait(10.Seconds());

            ScheduledMessageHandler.ReceivedMessages.Single()
                .Id.ShouldBe(2);
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }
    }

    public class ScheduledMessageApp : JasperRegistry
    {
        public ScheduledMessageApp() : base()
        {
            Publish.MessagesFromAssemblyContaining<ScheduledMessageApp>()
                .To("loopback://incoming");

            Transports.ListenForMessagesFrom("loopback://incoming");

            Logging.UseConsoleLogging = true;
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
            _source?.SetResult(message);

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

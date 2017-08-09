using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Jasper.Bus;
using Jasper.Testing.Bus.Bootstrapping;
using NSubstitute;
using Shouldly;
using Xunit;

namespace IntegrationTests.DelayedJobs
{
    public class end_to_end_with_in_memory : IDisposable
    {
        private JasperRuntime theRuntime;

        public end_to_end_with_in_memory()
        {
            DelayedMessageHandler.Reset();

            theRuntime = JasperRuntime.For<DelayedMessageApp>();
        }

        [Fact]
        public void send_in_a_delayed_message()
        {
            var message1 = new DelayedMessage{Id = 1};
            var message2 = new DelayedMessage{Id = 2};
            var message3 = new DelayedMessage{Id = 3};

            var bus = theRuntime.Container.GetInstance<IServiceBus>();


            bus.DelaySend(message1, 2.Hours());
            bus.DelaySend(message2, 5.Seconds());

            bus.DelaySend(message3, 2.Hours());




            DelayedMessageHandler.ReceivedMessages.Count.ShouldBe(0);

            DelayedMessageHandler.Received.Wait(10.Seconds());

            DelayedMessageHandler.ReceivedMessages.Single()
                .Id.ShouldBe(2);
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }
    }

    public class DelayedMessageApp : JasperRegistry
    {
        public DelayedMessageApp() : base()
        {
            Messaging.SendFromAssemblyContaining<DelayedMessageApp>()
                .To("loopback://incoming");

            Channels.ListenForMessagesFrom("loopback://incoming");

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
            _source?.SetResult(message);

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

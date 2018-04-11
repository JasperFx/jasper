using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Scheduled
{
    public class end_to_end_with_in_memory : IDisposable
    {
        private JasperRuntime theRuntime;
        private ScheduledMessageReceiver theReceiver;

        public end_to_end_with_in_memory()
        {

        }

        private async Task withApp()
        {
            var registry = new ScheduledMessageApp();
            theReceiver = registry.Receiver;

            theRuntime = await JasperRuntime.ForAsync(registry);
        }

        //[Fact] //-- TODO too stupidly unreliable for CI. REplace w/ ST
        public async Task run_scheduled_job_locally()
        {
            await withApp();

            var message1 = new ScheduledMessage{Id = 1};
            var message2 = new ScheduledMessage{Id = 2};
            var message3 = new ScheduledMessage{Id = 3};


            await theRuntime.Messaging.Schedule(message1, 2.Hours());
            await theRuntime.Messaging.Schedule(message2, 5.Seconds());

            await theRuntime.Messaging.Schedule(message3, 2.Hours());




            theReceiver.ReceivedMessages.Count.ShouldBe(0);

            theReceiver.Received.Wait(30.Seconds());

            theReceiver.ReceivedMessages.Single()
                .Id.ShouldBe(2);
        }

        // [Fact] TODO -- get this back. Unreliable in CI
        public async Task send_in_a_delayed_message()
        {
            await withApp();

            var message1 = new ScheduledMessage{Id = 1};
            var message2 = new ScheduledMessage{Id = 2};
            var message3 = new ScheduledMessage{Id = 3};


            await theRuntime.Messaging.ScheduleSend(message1, 2.Hours());
            await theRuntime.Messaging.ScheduleSend(message2, 5.Seconds());

            await theRuntime.Messaging.ScheduleSend(message3, 2.Hours());




            theReceiver.ReceivedMessages.Count.ShouldBe(0);

            await theReceiver.Received;

            theReceiver.ReceivedMessages.Single()
                .Id.ShouldBe(2);
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }
    }

    public class ScheduledMessageApp : JasperRegistry
    {
        public readonly ScheduledMessageReceiver Receiver = new ScheduledMessageReceiver();

        public ScheduledMessageApp() : base()
        {
            Services.AddSingleton(Receiver);

            Publish.MessagesFromAssemblyContaining<ScheduledMessageApp>()
                .To("loopback://incoming");

            Transports.ListenForMessagesFrom("loopback://incoming");

            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<ScheduledMessageCatcher>();
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
            _receiver.Source.SetResult(message);

            _receiver.ReceivedMessages.Add(message);
        }



    }
}

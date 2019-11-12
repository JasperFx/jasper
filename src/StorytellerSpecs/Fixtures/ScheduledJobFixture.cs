using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{
    public class ScheduledJobFixture : Fixture
    {
        private ScheduledMessageReceiver theReceiver;
        private IHost theHost;

        public ScheduledJobFixture()
        {
            Title = "In Memory Scheduled Jobs";
        }

        public override void SetUp()
        {
            var registry = new ScheduledMessageApp();
            theReceiver = registry.Receiver;

            theHost = JasperHost.For(registry);
        }

        public override void TearDown()
        {
            theHost?.Dispose();
        }

        [FormatAs("Schedule message locally {id} for {seconds} seconds from now")]
        public Task ScheduleMessage(int id, int seconds)
        {
            return theHost.Get<IMessagePublisher>().Schedule(new ScheduledMessage {Id = id}, seconds.Seconds());
        }

        [FormatAs("Schedule send message {id} for {seconds} seconds from now")]
        public Task ScheduleSendMessage(int id, int seconds)
        {
            return theHost.Get<IMessagePublisher>().ScheduleSend(new ScheduledMessage {Id = id}, seconds.Seconds());
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
    }

    public class ScheduledMessageApp : JasperOptions
    {
        public readonly ScheduledMessageReceiver Receiver = new ScheduledMessageReceiver();

        public ScheduledMessageApp()
        {
            Services.AddSingleton(Receiver);

            Publish.MessagesFromAssemblyContaining<ScheduledMessageApp>()
                .To("loopback://incoming");

            Transports.ListenForMessagesFrom("loopback://incoming");

            Handlers.Discovery(x =>
            {
                x.DisableConventionalDiscovery();
                x.IncludeType<ScheduledMessageCatcher>();
            });
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

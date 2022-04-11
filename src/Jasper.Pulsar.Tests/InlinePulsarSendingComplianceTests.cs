using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Pulsar.Tests
{
    public class InlineSender : JasperOptions
    {

        public InlineSender()
        {
            var topic = Guid.NewGuid().ToString();
            TopicPath = $"persistent://public/default/{topic}";

            var replyPath = $"persistent://public/default/replies-{topic}";

            this.ConnectToLocalPulsar();

            this.ListenToPulsarTopic(replyPath).UseForReplies().ProcessInline();

            this.PublishAllMessages().ToPulsar(TopicPath).SendInline();
        }

        public string TopicPath { get; set; }
    }

    public class InlineReceiver : JasperOptions
    {
        public InlineReceiver(string topicPath)
        {
            this.ConnectToLocalPulsar();

            this.ListenToPulsarTopic(topicPath).ProcessInline();


        }
    }


    public class InlinePulsarSendingFixture : SendingComplianceFixture, IAsyncLifetime
    {

        public InlinePulsarSendingFixture() : base(null)
        {

        }

        public async Task InitializeAsync()
        {
            var sender = new InlineSender();
            OutboundAddress = PulsarEndpoint.UriFor(sender.TopicPath);

            var receiver = new InlineReceiver(sender.TopicPath);

            await ReceiverIs(receiver);

            await SenderIs(sender);


        }

        public override void BeforeEach()
        {
            // These tests are *far* more reliable with a cooldown
            Thread.Sleep(3.Seconds());
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

    }


    [Collection("acceptance")]
    public class InlinePulsarSendingComplianceTests : SendingCompliance<InlinePulsarSendingFixture>
    {

    }

}

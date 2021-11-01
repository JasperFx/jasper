using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Pulsar.Tests
{

    public class Sender : JasperOptions
    {

        public Sender()
        {
            var topic = Guid.NewGuid().ToString();
            TopicPath = $"persistent://public/default/compliance{topic}";
            var listener = $"persistent://public/default/replies{topic}";
            Endpoints.ConfigurePulsar(e => {});
            Endpoints.ListenToPulsarTopic(listener).UseForReplies();

        }

        public string TopicPath { get; set; }
    }

    public class Receiver : JasperOptions
    {
        public Receiver(string topicPath)
        {
            Endpoints.ConnectToLocalPulsar();
            Endpoints.ListenToPulsarTopic(topicPath);


        }
    }


    public class PulsarSendingFixture : SendingComplianceFixture, IAsyncLifetime
    {
        public PulsarSendingFixture() : base(null)
        {

        }

        public async Task InitializeAsync()
        {
            var sender = new Sender();
            OutboundAddress = PulsarEndpoint.UriFor(sender.TopicPath);

            await SenderIs(sender);

            var receiver = new Receiver(sender.TopicPath);

            await ReceiverIs(receiver);
        }

        public override void BeforeEach()
        {
            // A cooldown makes these tests far more reliable
            Thread.Sleep(3.Seconds());
        }

        public Task DisposeAsync()
        {

            return Task.CompletedTask;
        }
    }

    [Collection("acceptance")]
    public class PulsarSendingComplianceTests : SendingCompliance<PulsarSendingFixture>
    {

    }
}

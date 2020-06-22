using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using DotPulsar;
using DotPulsar.Internal;
using Jasper.Tracking;
using Jasper.Util;
using Newtonsoft.Json;
using Shouldly;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.DotPulsar.Tests
{
    public class InlineSender : JasperOptions
    {
        public const int Count = 0;
        public const string Server = "pulsar://localhost:6650";
        public InlineSender(string topic)
        {
            Endpoints.ConfigurePulsar(new PulsarClientBuilder()
                .ServiceUrl(new Uri(Server)));
            Endpoints.PublishAllMessages().ToPulsarTopic(new ProducerOptions(topic));
            Endpoints.ListenToPulsarTopic(Guid.NewGuid().ToString(), topic + "-reply").UseForReplies().ProcessInline();
        }
    }

    public class InlineReceiver : JasperOptions
    {
        public InlineReceiver(string topic)
        {
            Endpoints.ConfigurePulsar(new PulsarClientBuilder().ServiceUrl(new Uri(InlineSender.Server)));
            Endpoints.PublishAllMessages().ToPulsarTopic(topic + "-reply");
            Endpoints.ListenToPulsarTopic(Guid.NewGuid().ToString(), topic).ProcessInline();
        }
    }


    public class InlineDotPulsarSendingComplianceTests : SendingCompliance
    {
        public static string Topic { get; } = "persistent://public/default/inline-send-receive";

        public InlineDotPulsarSendingComplianceTests() : base(new Uri(Topic), 30.Seconds())
        {
            var sender = new InlineSender(Topic);

            SenderIs(sender);
            
            var receiver = new InlineReceiver(Topic);

            ReceiverIs(receiver);

            Thread.Sleep(2000);
        }

        [Fact]

        public async Task publish_failures_reported_to_caller()
        {
            _ = await theSender.TrackActivity(10.Seconds())
                .DoNotAssertOnExceptionsDetected()
                .DoNotAssertTimeout()
                .ExecuteAndWait(c =>
                {
                    var serializationException = Should.Throw<JsonSerializationException>(c.Publish(new PoisonEnvelop()));
                    serializationException.InnerException.ShouldBeOfType<PoisionMessageException>();
                    return Task.CompletedTask;
                });
        }
    }

}

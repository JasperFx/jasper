using System;
using System.Threading.Tasks;
using Baseline.Dates;
using DotPulsar;
using DotPulsar.Internal;
using Jasper.Tracking;
using Newtonsoft.Json;
using Shouldly;
using TestingSupport.Compliance;
using TestMessages;
using Xunit;

namespace Jasper.Pulsar.Tests
{
    public class PulsarSendingComplianceTestsShell
    {
        private static string Server = "pulsar://localhost:6650";

        public class Sender : JasperOptions
        {
            public const string Topic = "persistent://public/default/jasper-compliance";
            public static string ReplyTopic = $"{Topic}-reply";
            
            public Sender()
            {
                Endpoints.ConfigurePulsar(new PulsarClientBuilder()
                    .ServiceUrl(new Uri(Server)));
                Endpoints.PublishAllMessages().ToPulsarTopic(new ProducerOptions(Topic));
                Endpoints.ListenToPulsarTopic("sender", ReplyTopic).UseForReplies();
            }
        }

        public class FailureSender : JasperOptions
        {
            public const string Topic = "persistent://public/default/jasper-compliance";
            public FailureSender()
            {
                Endpoints.ConfigurePulsar(new PulsarClientBuilder().ServiceUrl(new Uri("pulsar://localhost:6651")));
                Endpoints.PublishAllMessages().ToPulsarTopic(new ProducerOptions(Topic));
            }
        }

        public class Receiver : JasperOptions
        {
            public Receiver(string topic)
            {
                Endpoints.ConfigurePulsar(new PulsarClientBuilder().ServiceUrl(new Uri(Server)));
                Endpoints.PublishAllMessages().ToPulsarTopic(Sender.ReplyTopic);
                Endpoints.ListenToPulsarTopic("receiver", topic);
            }
        }

        public class PulsarSendingComplianceTests : SendingCompliance
        {
            public PulsarSendingComplianceTests() : base(new Uri(Sender.Topic))
            {
                var sender = new Sender();

                SenderIs(sender);

                var receiver = new Receiver(Sender.Topic);

                ReceiverIs(receiver);
            }

            [Fact]

            public async Task publish_failures_reported_to_caller()
            {
                theSender = null;
                SenderIs<FailureSender>();

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

            [Fact]

            public async Task publish_succeeds()
            {
                _ = await theSender.TrackActivity(10.Seconds())
                    .DoNotAssertOnExceptionsDetected()
                    .DoNotAssertTimeout()
                    .ExecuteAndWait(c => c.Publish(new Message1()));
            }
        }
    }

    public class PoisionMessageException : Exception
    {
        public const string PoisonMessage = "Poison message";
        public PoisionMessageException() : base(PoisonMessage)
        {
            
        }
    }
    
    public class PoisonEnvelop : Envelope
    {
        public new byte[] Data => throw new PoisionMessageException();
    }
}

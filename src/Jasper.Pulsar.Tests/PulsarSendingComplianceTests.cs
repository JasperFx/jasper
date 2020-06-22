using System;
using System.Threading;
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

namespace Jasper.DotPulsar.Tests
{
    public class PulsarSendingComplianceTestsShell
    {
        // SAMPLE: PulsarSendReceiveExample
        private static string Server = "pulsar://localhost:6650";

        public class Sender : JasperOptions
        {
            public Sender(string topic)
            {
                Endpoints.ConfigurePulsar(new PulsarClientBuilder()
                    .ServiceUrl(new Uri(Server)));
                Endpoints.PublishAllMessages().ToPulsarTopic(new ProducerOptions(topic));
                Endpoints.ListenToPulsarTopic(Guid.NewGuid().ToString(), topic + "-reply").UseForReplies();
            }
        }

        public class Receiver : JasperOptions
        {
            public Receiver(string topic)
            {
                Endpoints.ConfigurePulsar(new PulsarClientBuilder().ServiceUrl(new Uri(Server)));
                Endpoints.PublishAllMessages().ToPulsarTopic(topic + "-reply");
                Endpoints.ListenToPulsarTopic(Guid.NewGuid().ToString(), topic);
            }
        }
        // ENDSAMPLE

        public class PulsarSendingComplianceTests : SendingCompliance
        {
            public static string Topic { get; } = "persistent://public/default/jasper";

            public PulsarSendingComplianceTests() : base(new Uri(Topic), 30.Seconds())
            {
                var sender = new Sender(Topic);

                SenderIs(sender);

                var receiver = new Receiver(Topic);

                ReceiverIs(receiver);

                Thread.Sleep(2000);
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

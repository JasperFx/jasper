using System;
using System.Threading.Tasks;
using Baseline.Dates;
using DotPulsar;
using DotPulsar.Internal;
using Jasper.Tracking;
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
                    .ExceptionHandler(context =>
                    {

                        return new ValueTask(Task.CompletedTask);
                    })
                    .ServiceUrl(new Uri(Server)));
                Endpoints.PublishAllMessages().ToPulsarTopic(new ProducerOptions(Topic));
                Endpoints.ListenToPulsarTopic("compliance-tests", ReplyTopic).UseForReplies();
            }
        }

        public class FailureSender : JasperOptions
        {
            public const string Topic = "persistent://public/default/jasper-compliance";
            public FailureSender()
            {
                Endpoints.ConfigurePulsar(new PulsarClientBuilder().ServiceUrl(new Uri(Server)));
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

                _ = await theSender.TrackActivity(60.Seconds())
                    .DoNotAssertOnExceptionsDetected()
                    .DoNotAssertTimeout()
                    .ExecuteAndWait(c =>
                    {
                        Should.Throw<Exception>(c.Publish(new Message1()));
                        return Task.CompletedTask;
                    });
            }

            [Fact]

            public async Task publish_succeeds()
            {
                _ = await theSender.TrackActivity(60.Seconds())
                    .DoNotAssertOnExceptionsDetected()
                    .DoNotAssertTimeout()
                    .ExecuteAndWait(c => c.Publish(new Message1()));
            }
        }
    }
}

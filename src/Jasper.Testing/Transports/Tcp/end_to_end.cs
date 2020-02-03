using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Runtime.Scheduled;
using Jasper.Tracking;
using Jasper.Util;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Transports.Tcp
{
    [Collection("integration")]
    public class end_to_end : IDisposable
    {
        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        private static int port = 2114;

        private IHost theSender;
        private readonly Uri theAddress = $"tcp://localhost:{++port}/incoming".ToUri();
        private IHost theReceiver;
        private FakeScheduledJobProcessor scheduledJobs;


        private void getReady()
        {
            theSender = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Extensions.UseMessageTrackingTestingSupport();
            });

            var receiver = new JasperOptions();
            receiver.Handlers.DisableConventionalDiscovery();

            receiver.Endpoints.ListenForMessagesFrom(theAddress);

            receiver.Handlers.Retries.MaximumAttempts = 3;
            receiver.Handlers.IncludeType<MessageConsumer>();

            scheduledJobs = new FakeScheduledJobProcessor();

            receiver.Services.For<IScheduledJobProcessor>().Use(scheduledJobs);

            receiver.Extensions.UseMessageTrackingTestingSupport();

            theReceiver = JasperHost.For(receiver);
        }

        [Fact]
        public async Task can_apply_requeue_mechanics()
        {
            getReady();

            var session = await theSender.TrackActivity()
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .ExecuteAndWait(c => c.SendToDestination(theAddress, new Message2()));



            session.FindSingleTrackedMessageOfType<Message2>(EventType.MessageSucceeded)
                .ShouldNotBeNull();

        }

        [Fact]
        public async Task can_send_from_one_node_to_another()
        {
            getReady();

            var session = await theSender.TrackActivity()
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .ExecuteAndWait(c => c.SendToDestination(theAddress, new Message1()));


            session.FindSingleTrackedMessageOfType<Message1>(EventType.MessageSucceeded)
                .ShouldNotBeNull();
        }

        [Fact]
        public async Task tags_the_envelope_with_the_source()
        {
            getReady();

            var session = await theSender.TrackActivity()
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .ExecuteAndWait(c => c.SendToDestination(theAddress, new Message2()));


            var record = session.FindEnvelopesWithMessageType<Message2>(EventType.MessageSucceeded).Single();
            record
                .ShouldNotBeNull();

            record.Envelope.Source.ShouldBe(theSender.Get<JasperOptions>().ServiceName);
        }
    }
}

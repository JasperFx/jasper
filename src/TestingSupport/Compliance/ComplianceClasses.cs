using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper;
using Jasper.Tracking;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestMessages;
using Xunit;

namespace TestingSupport.Compliance
{
    public abstract class SendingCompliance : IDisposable
    {
        private IHost theSender;
        private IHost theReceiver;
        protected Uri theAddress;

        protected SendingCompliance(Uri destination)
        {
            theAddress = destination;
        }

        public void SenderIs<T>() where T : JasperOptions, new()
        {
            theSender = JasperHost.For<T>(configureSender);
        }

        private static void configureSender<T>(T options) where T : JasperOptions, new()
        {
            options.Handlers.DisableConventionalDiscovery();
            options.Extensions.UseMessageTrackingTestingSupport();
        }

        public void ReceiverIs<T>() where T : JasperOptions, new()
        {
            theReceiver = JasperHost.For<T>(configureReceiver);
        }

        private static void configureReceiver<T>(T options) where T : JasperOptions, new()
        {
            options.Handlers.DisableConventionalDiscovery();

            options.Handlers.Retries.MaximumAttempts = 3;
            options.Handlers.IncludeType<MessageConsumer>();

            options.Extensions.UseMessageTrackingTestingSupport();
        }

        public void SenderIs(Action<JasperOptions> configure)
        {
            theSender = JasperHost.For(x =>
            {
                configure(x);
                configureSender(x);
            });
        }

        public void ReceiverIs(Action<JasperOptions> configure)
        {
            theReceiver = JasperHost.For(x =>
            {
                configure(x);
                configureReceiver(x);
            });
        }

        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        [Fact]
        public async Task can_apply_requeue_mechanics()
        {

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

    public class TimeoutsMessage
    {
    }
}

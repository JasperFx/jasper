using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Jasper.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestMessages;
using Xunit;

namespace TestingSupport.Compliance
{
    /*
     * TODOs
     * Error Handling
     * Request/Response

     * Ping?
     */

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

        private void configureSender<T>(T options) where T : JasperOptions, new()
        {
            options.Handlers.DisableConventionalDiscovery();
            options.Extensions.UseMessageTrackingTestingSupport();
            options.ServiceName = "SenderService";
            options.Endpoints.PublishAllMessages().To(theAddress);
        }

        public void ReceiverIs<T>() where T : JasperOptions, new()
        {
            theReceiver = JasperHost.For<T>(configureReceiver);
        }

        private static void configureReceiver<T>(T options) where T : JasperOptions, new()
        {

            options.Handlers.Retries.MaximumAttempts = 3;
            options.Handlers
                .DisableConventionalDiscovery()
                .IncludeType<MessageConsumer>()
                .IncludeType<ExecutedMessageGuy>()
                .IncludeType<ColorHandler>();

            options.Extensions.UseMessageTrackingTestingSupport();

            options.Services.AddSingleton(new ColorHistory());
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
        public async Task can_send_from_one_node_to_another_by_destination()
        {
            var session = await theSender.TrackActivity()
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .ExecuteAndWait(c => c.SendToDestination(theAddress, new Message1()));


            session.FindSingleTrackedMessageOfType<Message1>(EventType.MessageSucceeded)
                .ShouldNotBeNull();
        }

        [Fact]
        public async Task can_send_from_one_node_to_another_by_publishing_rule()
        {
            var message1 = new Message1();

            var session = await theSender.TrackActivity()
                .AlsoTrack(theReceiver)
                .DoNotAssertOnExceptionsDetected()
                .SendMessageAndWait(message1);


            session.FindSingleTrackedMessageOfType<Message1>(EventType.MessageSucceeded)
                .Id.ShouldBe(message1.Id);
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

        [Fact]
        public async Task tracking_correlation_id_on_everything()
        {

                var id2 = Guid.Empty;
                var session2 = await theSender
                    .TrackActivity()
                    .AlsoTrack(theReceiver)

                    .ExecuteAndWait(async context =>
                {
                    id2 = context.CorrelationId;

                    await context.Send(new ExecutedMessage());
                    await context.Publish(new ExecutedMessage());
                    //await context.ScheduleSend(new ExecutedMessage(), DateTime.UtcNow.AddDays(5));
                });

                var envelopes = session2
                    .AllRecordsInOrder(EventType.Sent)
                    .Select(x => x.Envelope)
                    .ToArray();


                foreach (var envelope in envelopes) envelope.CorrelationId.ShouldBe(id2);
        }

        [Fact]
        public async Task schedule_send()
        {
            await theSender
                .TrackActivity()
                .AlsoTrack(theReceiver)
                .Timeout(15.Seconds())
                .ExecuteAndWait(c => c.ScheduleSend(new ColorChosen {Name = "Orange"}, 5.Seconds()));

            theReceiver.Get<ColorHistory>().Name.ShouldBe("Orange");
        }
    }


}

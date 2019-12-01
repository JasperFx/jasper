using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Tracking;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Tcp
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
        private readonly MessageTracker theTracker = new MessageTracker();
        private IHost theReceiver;
        private FakeScheduledJobProcessor scheduledJobs;


        private void getReady()
        {
            theSender = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Services.AddSingleton(theTracker);
            });

            var receiver = new JasperOptions();
            receiver.Handlers.DisableConventionalDiscovery();

            receiver.Transports.ListenForMessagesFrom(theAddress);

            receiver.Handlers.Retries.MaximumAttempts = 3;
            receiver.Handlers.IncludeType<MessageConsumer>();

            scheduledJobs = new FakeScheduledJobProcessor();

            receiver.Services.For<IScheduledJobProcessor>().Use(scheduledJobs);

            receiver.Services.For<MessageTracker>().Use(theTracker);

            theReceiver = JasperHost.For(receiver);
        }

        [Fact]
        public async Task can_apply_requeue_mechanics()
        {
            getReady();

            var waiter = theTracker.WaitFor<Message2>();

            await theSender.Get<IMessagePublisher>().Send(theAddress, new Message2());

            var env = await waiter;

            env.Message.ShouldBeOfType<Message2>();
        }

        [Fact]
        public async Task can_send_from_one_node_to_another()
        {
            getReady();

            var waiter = theTracker.WaitFor<Message1>();

            await theSender.Get<IMessagePublisher>().Send(theAddress, new Message1());

            var env = await waiter;

            env.Message.ShouldBeOfType<Message1>();
        }

        [Fact]
        public async Task tags_the_envelope_with_the_source()
        {
            getReady();

            var waiter = theTracker.WaitFor<Message2>();

            await theSender.Get<IMessagePublisher>().Send(theAddress, new Message2());

            var env = await waiter;

            env.Source.ShouldBe(theSender.Get<JasperOptions>().ServiceName);
        }
    }
}

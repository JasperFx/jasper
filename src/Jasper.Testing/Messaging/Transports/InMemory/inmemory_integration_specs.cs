using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Testing.Messaging.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.InMemory
{
    public class inmemory_integration_specs : IntegrationContext
    {
        private readonly MessageTracker theTracker = new MessageTracker();

        private Task configure()
        {
            return with(_ =>
            {
                _.Publish.Message<Message1>().To("loopback://incoming");

                _.Services.AddSingleton(theTracker);

                _.Services.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.WithDefaultConventions();
                });
            });
        }

        [Fact]
        public async Task automatically_sticks_in_replies_queue()
        {
            await configure();
            Channels.HasChannel(TransportConstants.RetryUri)
                .ShouldBeTrue();
        }


        [Fact]
        public async Task send_a_message_and_get_the_response()
        {
            await configure();

            var bus = Runtime.Get<IMessageContext>();

            var waiter = theTracker.WaitFor<Message1>();

            await bus.Send(new Message1());

            await waiter;

            if (!waiter.IsCompleted)
            {
                throw new Exception("Got no envelope!");
            }

            var envelope = waiter.Result;

            envelope.ShouldNotBeNull();
        }

    }
}

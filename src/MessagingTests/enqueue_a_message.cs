using System;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Tracking;
using MessagingTests.Lightweight;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.Routing.Handlers;
using Shouldly;
using TestingSupport;
using TestingSupport.Fakes;
using TestMessages;
using Xunit;

namespace MessagingTests
{
    public class enqueue_a_message
    {
        [Fact]
        public async Task enqueue_locally()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery();

            registry.Services.Scan(x =>
            {
                x.TheCallingAssembly();
                x.WithDefaultConventions();
            });
            registry.Handlers.IncludeType<MessageConsumer>();

            registry.Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();

            var tracker = new MessageTracker();
            registry.Services.AddSingleton(tracker);

            using (var runtime = JasperHost.For(registry))
            {
                var waiter = tracker.WaitFor<Message1>();
                var message = new Message1
                {
                    Id = Guid.NewGuid()
                };

                await runtime.Get<IMessageContext>().Enqueue(message);

                var received = await waiter;

                received.Message.As<Message1>().Id.ShouldBe(message.Id);
            }
        }


        [Fact]
        public async Task enqueue_locally_lightweight()
        {
            var registry = new JasperRegistry();


            registry.Handlers.IncludeType<RecordCallHandler>();
            registry.Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();

            var tracker = new MessageTracker();
            registry.Services.AddSingleton(tracker);

            using (var runtime = JasperHost.For(registry))
            {
                var waiter = tracker.WaitFor<Message1>();
                var message = new Message1
                {
                    Id = Guid.NewGuid()
                };

                await runtime.Get<IMessageContext>().EnqueueLightweight(message);

                waiter.Wait(5.Seconds());
                var received = waiter.Result;

                received.Message.As<Message1>().Id.ShouldBe(message.Id);
            }
        }

        [Fact]
        public async Task enqueue_locally_with_designated_worker_queue()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery();

            registry.Services.Scan(x =>
            {
                x.TheCallingAssembly();
                x.WithDefaultConventions();
            });
            registry.Handlers.IncludeType<MessageConsumer>();

            registry.Handlers.Worker("foo").MaximumParallelization(3);

            var tracker = new MessageTracker();
            registry.Services.AddSingleton(tracker);

            using (var runtime = JasperHost.For(registry))
            {
                var waiter = tracker.WaitFor<Message1>();
                var message = new Message1
                {
                    Id = Guid.NewGuid()
                };

                await runtime.Get<IMessageContext>().Enqueue(message, "foo");

                var received = await waiter;

                received.Message.As<Message1>().Id.ShouldBe(message.Id);
            }

        }
    }
}

using System;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Messaging.Compilation;
using Jasper.Testing.Messaging.Runtime;
using Jasper.Testing.Samples.HandlerDiscovery;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.Routing.Handlers;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    [Collection("integration")]
    public class enqueue_a_message
    {
        [Fact]
        public async Task enqueue_locally()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery(false);

            registry.Services.Scan(x =>
            {
                x.TheCallingAssembly();
                x.WithDefaultConventions();
            });
            registry.Handlers.IncludeType<RecordCallHandler>();
            registry.Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();
            registry.Services.AddTransient<IFakeService, FakeService>();
            registry.Services.AddTransient<IWidget, Widget>();

            var tracker = new MessageTracker();
            registry.Services.AddSingleton(tracker);

            using (var runtime = JasperRuntime.For(registry))
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
            registry.Services.AddTransient<IFakeService, FakeService>();
            registry.Services.AddTransient<IWidget, Widget>();
            registry.Services.AddTransient<IMyService, MyService>();
            registry.Services.AddTransient<IPongWriter, PongWriter>();

            var tracker = new MessageTracker();
            registry.Services.AddSingleton(tracker);

            using (var runtime = JasperRuntime.For(registry))
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
    }
}

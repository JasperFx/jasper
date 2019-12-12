using System;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Messaging.Tracking;
using Jasper.Testing.Messaging.Transports.Tcp;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.Routing.Handlers;
using Shouldly;
using TestingSupport;
using TestingSupport.Fakes;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class enqueue_a_message
    {
        [Fact]
        public async Task enqueue_locally()
        {
            var registry = new JasperOptions();
            registry.Handlers.DisableConventionalDiscovery();

            registry.Services.Scan(x =>
            {
                x.TheCallingAssembly();
                x.WithDefaultConventions();
            });
            registry.Handlers.IncludeType<MessageConsumer>();

            registry.Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();
            registry.Extensions.UseMessageTrackingTestingSupport();

            using (var host = JasperHost.For(registry))
            {
                var message = new Message1
                {
                    Id = Guid.NewGuid()
                };

                var session = await host.ExecuteAndWait(c => c.Enqueue(message));

                var tracked = session.FindSingleTrackedMessageOfType<Message1>();

                tracked.Id.ShouldBe(message.Id);
            }
        }


        [Fact]
        public async Task enqueue_locally_lightweight()
        {
            var registry = new JasperOptions();


            registry.Handlers.IncludeType<RecordCallHandler>();
            registry.Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();

            registry.Extensions.UseMessageTrackingTestingSupport();

            using (var host = JasperHost.For(registry))
            {
                var message = new Message1
                {
                    Id = Guid.NewGuid()
                };

                var session = await host.ExecuteAndWait(c => c.Enqueue(message));

                var tracked = session.FindSingleTrackedMessageOfType<Message1>();

                tracked.Id.ShouldBe(message.Id);
            }
        }


    }
}

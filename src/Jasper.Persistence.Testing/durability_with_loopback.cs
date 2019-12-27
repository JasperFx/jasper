using System;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.ErrorHandling;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Testing.Marten;
using Jasper.Runtime.Handlers;
using Jasper.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing
{

    public class durability_with_loopback : PostgresqlContext
    {
        [Fact]
        public async Task should_recover_persisted_messages()
        {
            using (var host1 = JasperHost.For(new DurableSender(true)))
            {
                host1.RebuildMessageStorage();

                await host1.Send(new ReceivedMessage());

                var counts = await host1.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();

                await host1.StopAsync();

                counts.Incoming.ShouldBe(1);
            }

            using (var host1 = JasperHost.For(new DurableSender(true)))
            {
                var counts = await host1.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();

                var i = 0;
                while (counts.Incoming != 1 && i < 10)
                {
                    await Task.Delay(100.Milliseconds());
                    counts = await host1.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();
                }

                counts.Incoming.ShouldBe(1);


                await host1.StopAsync();
            }
        }
    }

    public class DurableSender : JasperOptions
    {
        public DurableSender(bool latched)
        {
            Endpoints.PublishAllMessages()
                .ToLocalQueue("one")
                .Durably();

            Extensions.UseMarten(Servers.PostgresConnectionString);

            Services.AddSingleton(new ReceivingSettings {Latched = true});

            Extensions.UseMessageTrackingTestingSupport();
        }
    }

    public class ReceivingSettings
    {
        public bool Latched { get; set; } = true;
    }

    public class ReceivedMessageHandler
    {
        public void Handle(ReceivedMessage message, Envelope envelope, ReceivingSettings settings)
        {
            if (settings.Latched) throw new DivideByZeroException();

        }

        public static void Configure(HandlerChain chain)
        {
            chain.Retries.Add(x => x.Handle<Exception>(e => true).Requeue(1000));
        }
    }

    public class ReceivedMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }


}

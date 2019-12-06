using System;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Messaging.Durability;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Testing.Marten;
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
            using (var runtime1 = JasperHost.For(new DurableSender(true)))
            {
                runtime1.RebuildMessageStorage();

                await runtime1.Send(new ReceivedMessage());

                var counts = await runtime1.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();

                await runtime1.StopAsync();

                counts.Incoming.ShouldBe(1);
            }

            using (var runtime1 = JasperHost.For(new DurableSender(true)))
            {
                var counts = await runtime1.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();

                counts.Incoming.ShouldBe(1);

                var tracker = runtime1.Get<MessageTracker>();

                var waiter = tracker.WaitFor<ReceivedMessage>();

                runtime1.Get<ReceivingSettings>().Latched = false;

                (await waiter).ShouldNotBeNull();

                await runtime1.StopAsync();
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

            Settings.Alter<ReceivingSettings>(x => x.Latched = latched);

            Services.AddSingleton<MessageTracker>();
        }
    }

    public class ReceivingSettings
    {
        public bool Latched { get; set; } = true;
    }

    public class ReceivedMessageHandler
    {
        public void Handle(ReceivedMessage message, Envelope envelope, ReceivingSettings settings, MessageTracker tracker)
        {
            if (settings.Latched) throw new DivideByZeroException();

            tracker.Record(message, envelope);
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

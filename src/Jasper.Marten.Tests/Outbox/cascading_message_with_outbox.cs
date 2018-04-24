using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Marten.Tests.Setup;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Testing;
using Jasper.Testing.Messaging;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Outbox
{
    public class OutboxSender : JasperRegistry
    {
        public OutboxSender(MessageTracker tracker)
        {

            Handlers.DisableConventionalDiscovery().IncludeType<CascadeReceiver>();
            Services.AddSingleton(tracker);
            Publish.Message<TriggerMessage>().To("durable://localhost:2337");
            Transports.DurableListenerAt(2338);

            Settings.ConfigureMarten(marten =>
            {
                marten.Connection(ConnectionSource.ConnectionString);
                marten.DatabaseSchemaName = "outbox_sender";
            });

            Include<MartenBackedPersistence>();
        }
    }

    public class OutboxReceiver : JasperRegistry
    {
        public OutboxReceiver()
        {

            Handlers.DisableConventionalDiscovery().IncludeType<TriggerMessageReceiver>();
            Settings.ConfigureMarten(marten =>
            {
                marten.Connection(ConnectionSource.ConnectionString);
                marten.DatabaseSchemaName = "outbox_receiver";
            });

            Include<MartenBackedPersistence>();

            Transports.DurableListenerAt(2337);
        }
    }


    public class cascading_message_with_outbox : IDisposable
    {
        public cascading_message_with_outbox()
        {
            theSender = JasperRuntime.For(new OutboxSender(theTracker));
            theReceiver = JasperRuntime.For<OutboxReceiver>();
        }

        public void Dispose()
        {
            theReceiver?.Dispose();
            theSender?.Dispose();
        }

        private readonly MessageTracker theTracker = new MessageTracker();
        private readonly JasperRuntime theReceiver;
        private readonly JasperRuntime theSender;

        [Fact]
        public async Task send_end_to_end_and_back_with_cascading_message()
        {
            var trigger = new TriggerMessage {Name = "Ronald"};

            var waiter = theTracker.WaitFor<CascadedMessage>();

            await theSender.Messaging.Send(trigger);

            waiter.Wait(10.Seconds());

            waiter.Result.ShouldNotBeNull();
            waiter.Result.Message.ShouldBeOfType<CascadedMessage>()
                .Name.ShouldBe("Ronald");
        }
    }

    public class TriggerMessage
    {
        public string Name { get; set; }
    }

    public class CascadedMessage
    {
        public string Name { get; set; }
    }

    public class CascadeReceiver
    {
        public void Handle(CascadedMessage message, MessageTracker tracker, Envelope envelope)
        {
            tracker.Record(message, envelope);
        }
    }

    public class TriggerMessageReceiver
    {
        [MartenTransaction]
        public object Handle(TriggerMessage message, IDocumentSession session, IMessageContext context)
        {
            var response = new CascadedMessage
            {
                Name = message.Name
            };

            return new RespondToSender(response);
        }
    }
}

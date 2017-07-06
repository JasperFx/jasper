using System;
using System.Threading.Tasks;
using IntegrationTests.Lightweight;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace IntegrationTests.Conneg
{
    public class sending_messages_without_sharing_types : IDisposable
    {
        private JasperRuntime greenApp;
        private JasperRuntime blueApp;
        private MessageTracker theTracker;

        public sending_messages_without_sharing_types()
        {
            greenApp = JasperRuntime.For<GreenApp>();
            blueApp = JasperRuntime.For<BlueApp>();

            theTracker = greenApp.Container.GetInstance<MessageTracker>();
        }

        public void Dispose()
        {
            greenApp?.Dispose();
            blueApp?.Dispose();
        }

        [Fact]
        public async Task send_green_that_gets_received_as_blue()
        {
            var waiter = theTracker.WaitFor<BlueMessage>();

            greenApp.Container.GetInstance<IServiceBus>().Send(new GreenMessage {Name = "Kareem Abdul Jabbar"});

            var envelope = await waiter;


            envelope.Message
                .ShouldBeOfType<BlueMessage>()
                .Name.ShouldBe("Kareem Abdul Jabbar");
        }


    }

    public class BlueApp : JasperBusRegistry
    {
        public BlueApp()
        {
            ListenForMessagesFrom("jasper://localhost:2555/blue");
        }
    }

    public class GreenApp : JasperBusRegistry
    {
        public GreenApp()
        {
            Services.For<MessageTracker>().Use(new MessageTracker());

            SendMessage<GreenMessage>().To("jasper://localhost:2555/blue");
        }
    }

    [TypeAlias("Structural.Typed.Message")]
    public class BlueMessage
    {
        public string Name { get; set; }
    }

    [TypeAlias("Structural.Typed.Message")]
    public class GreenMessage
    {
        public string Name { get; set; }
    }

    public class BlueHandler
    {
        private readonly MessageTracker _tracker;

        public BlueHandler(MessageTracker tracker)
        {
            _tracker = tracker;
        }

        public void Consume(Envelope envelope, BlueMessage message)
        {
            _tracker.Record(message, envelope);
        }
    }
}

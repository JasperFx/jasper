using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests.Lightweight;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Conneg;
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

            theTracker = blueApp.Container.GetInstance<MessageTracker>();
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

            await greenApp.Container.GetInstance<IServiceBus>().Send(new GreenMessage {Name = "Kareem Abdul Jabbar"});

            var envelope = await waiter;


            envelope.Message
                .ShouldBeOfType<BlueMessage>()
                .Name.ShouldBe("Kareem Abdul Jabbar");
        }

        [Fact]
        public async Task send_green_as_text_and_receive_as_blue()
        {
            var waiter = theTracker.WaitFor<BlueMessage>();

            await greenApp.Container.GetInstance<IServiceBus>()
                .Send(new GreenMessage {Name = "Magic Johnson"}, _ => _.ContentType = "text/plain");

            var envelope = await waiter;


            envelope.Message
                .ShouldBeOfType<BlueMessage>()
                .Name.ShouldBe("Magic Johnson");
        }


    }

    public class GreenTextWriter : IMediaWriter
    {
        public Type DotNetType { get; } = typeof(GreenMessage);
        public string ContentType { get; } = "text/plain";
        public byte[] Write(object model)
        {
            var name = model.As<GreenMessage>().Name;
            return Encoding.UTF8.GetBytes(name);
        }

        public Task Write(object model, Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class BlueTextReader : IMediaReader
    {
        public string MessageType { get; } = typeof(BlueMessage).ToTypeAlias();
        public Type DotNetType { get; } = typeof(BlueMessage);
        public string ContentType { get; } = "text/plain";
        public object Read(byte[] data)
        {
            var name = Encoding.UTF8.GetString(data);
            return new BlueMessage {Name = name};
        }

        public Task<T> Read<T>(Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class BlueApp : JasperBusRegistry
    {
        public BlueApp()
        {
            Services.For<MessageTracker>().Use(new MessageTracker());
            ListenForMessagesFrom("jasper://localhost:2555/blue");
        }
    }

    public class GreenApp : JasperBusRegistry
    {
        public GreenApp()
        {
            SendMessage<GreenMessage>().To("jasper://localhost:2555/blue");

            Channel("jasper://localhost:2555/blue").AcceptedContentTypes("text/plain");
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

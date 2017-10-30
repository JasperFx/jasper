using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Conneg;
using Jasper.Testing;
using Jasper.Testing.Bus;
using Jasper.Testing.Bus.Lightweight;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
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
            theTracker = new MessageTracker();

            greenApp = JasperRuntime.For<GreenApp>();
            blueApp = JasperRuntime.For(new BlueApp(theTracker));



            theTracker.ShouldBeTheSameAs(blueApp.Get<MessageTracker>());
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

            await greenApp.Bus.Send(new GreenMessage {Name = "Kareem Abdul Jabbar"});

            var envelope = await waiter;


            envelope.Message
                .ShouldBeOfType<BlueMessage>()
                .Name.ShouldBe("Kareem Abdul Jabbar");
        }

        [Fact]
        public async Task send_green_as_text_and_receive_as_blue()
        {
            var waiter = theTracker.WaitFor<BlueMessage>();

            await greenApp.Bus
                .Send(new GreenMessage {Name = "Magic Johnson"}, _ => _.ContentType = "text/plain");

            var envelope = await waiter;


            envelope.Message
                .ShouldBeOfType<BlueMessage>()
                .Name.ShouldBe("Magic Johnson");
        }


    }

    // SAMPLE: GreenTextWriter
    public class GreenTextWriter : MessageSerializerBase<GreenMessage>
    {
        public GreenTextWriter() : base("text/plain")
        {
        }

        public override byte[] Write(GreenMessage model)
        {
            return Encoding.UTF8.GetBytes(model.Name);
        }

        public override Task WriteToStream(GreenMessage model, HttpResponse response)
        {
            return response.WriteAsync(model.Name);
        }
    }
    // ENDSAMPLE

    // SAMPLE: BlueTextReader
    public class BlueTextReader : MessageDeserializerBase<BlueMessage>
    {
        public BlueTextReader() : base("text/plain")
        {
        }

        public override BlueMessage ReadData(byte[] data)
        {
            var name = Encoding.UTF8.GetString(data);
            return new BlueMessage {Name = name};
        }

        protected override async Task<BlueMessage> ReadData(Stream stream)
        {
            var name = await stream.ReadAllTextAsync();
            return new BlueMessage{Name = name};
        }
    }
    // ENDSAMPLE

    public class BlueApp : JasperRegistry
    {
        public BlueApp(MessageTracker tracker)
        {
            Services.ForSingletonOf<MessageTracker>().Use(tracker);
            Transports.ListenForMessagesFrom("tcp://localhost:2555/blue");
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<BlueHandler>();
        }
    }

    public class GreenApp : JasperRegistry
    {
        public GreenApp()
        {
            Publish.Message<GreenMessage>().To("tcp://localhost:2555/blue");
            Handlers.DisableConventionalDiscovery();
        }
    }

    [MessageAlias("Structural.Typed.Message")]
    public class BlueMessage
    {
        public string Name { get; set; }
    }

    [MessageAlias("Structural.Typed.Message")]
    public class GreenMessage
    {
        public string Name { get; set; }
    }

    public class BlueHandler
    {
        public static void Consume(Envelope envelope, BlueMessage message, MessageTracker tracker)
        {
            tracker.Record(message, envelope);
        }
    }
}

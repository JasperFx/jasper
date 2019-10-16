﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Jasper.Conneg;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Util;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class sending_messages_without_sharing_types : IDisposable
    {
        public sending_messages_without_sharing_types()
        {
            theTracker = new MessageTracker();
        }

        public void Dispose()
        {
            greenApp?.Dispose();
            blueApp?.Dispose();
        }

        private IJasperHost greenApp;
        private IJasperHost blueApp;
        private readonly MessageTracker theTracker;

        [Fact]
        public async Task send_green_as_text_and_receive_as_blue()
        {
            greenApp = JasperHost.For<GreenApp>();
            blueApp = JasperHost.For(new BlueApp(theTracker));


            theTracker.ShouldBeSameAs(blueApp.Get<MessageTracker>());

            var waiter = theTracker.WaitFor<BlueMessage>();

            await greenApp.Messaging
                .Send(new GreenMessage {Name = "Magic Johnson"}, _ => _.ContentType = "text/plain");

            var envelope = await waiter;


            envelope.Message
                .ShouldBeOfType<BlueMessage>()
                .Name.ShouldBe("Magic Johnson");
        }

        [Fact]
        public async Task send_green_that_gets_received_as_blue()
        {
            greenApp = JasperHost.For<GreenApp>();
            blueApp = JasperHost.For(new BlueApp(theTracker));

            var waiter = theTracker.WaitFor<BlueMessage>();

            await greenApp.Messaging.Send(new GreenMessage {Name = "Kareem Abdul Jabbar"});

            var envelope = await waiter;


            envelope.Message
                .ShouldBeOfType<BlueMessage>()
                .Name.ShouldBe("Kareem Abdul Jabbar");
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
            return new BlueMessage {Name = name};
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

    [MessageIdentity("Structural.Typed.Message")]
    public class BlueMessage
    {
        public string Name { get; set; }
    }

    [MessageIdentity("Structural.Typed.Message")]
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

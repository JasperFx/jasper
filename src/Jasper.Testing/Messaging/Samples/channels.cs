using System;
using System.Reflection;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Util;

namespace Jasper.Testing.Messaging.Samples
{
    // SAMPLE: SampleSettings
    public class SampleSettings
    {
        public Uri Pinger { get; set; } =
            "durable://localhost:2352/pinger".ToUri();

        public Uri Ponger { get; set; } =
            "durable://localhost:2353/ponger".ToUri();
    }
    // ENDSAMPLE

    // SAMPLE: PingApp
    public class PingApp : JasperRegistry
    {
        public PingApp(SampleSettings settings)
        {
            // Configuring PingApp to send PingMessage's
            // to the PongApp
            Publish.Message<PingMessage>()
                .To(settings.Pinger);

            // Listen for incoming messages from "Pinger"
            Transports.ListenForMessagesFrom(settings.Pinger);
        }
    }

    public class PongApp : JasperRegistry
    {
        public PongApp(SampleSettings settings)
        {
            // Listen for incoming messages from "Ponger"
            Transports.ListenForMessagesFrom(settings.Ponger);
        }
    }
    // ENDSAMPLE

    public class PingMessage
    {
    }

    public class PongMessage
    {
    }

    // SAMPLE: PingPongHandler
    public class PingPongHandler
    {
        public PongMessage Handle(PingMessage message)
        {
            return new PongMessage();
        }
    }
    // ENDSAMPLE

    // SAMPLE: ListeningApp
    public class ListeningApp : JasperRegistry
    {
        public ListeningApp(SampleSettings settings)
        {
            // Listen for incoming messages from "Pinger"
            Transports.ListenForMessagesFrom(settings.Pinger);
        }
    }
    // ENDSAMPLE

    // SAMPLE: PersistentMessageChannels
    public class AppSettings
    {
        // This channel handles "fire and forget"
        // control messages
        public Uri Control { get; set; }
            = new Uri("durable://localhost:2345/control");


        // This channel handles normal business
        // processing messages
        public Uri Transactions { get; set; }
            = new Uri("durable://localhost:2346/transactions");
    }

    // ENDSAMPLE

    // SAMPLE: sending-messages-for-static-routing
    public class SendingExample
    {
        public async Task SendPingsAndPongs(IMessageContext bus)
        {
            // Publish a message
            await bus.Send(new PingMessage());

            // Request/Reply
            var pong = await bus.Request<PongMessage>(new PingMessage());
        }
    }
    // ENDSAMPLE


    public class PingPong
    {
        // SAMPLE: using-request-reply
        public async Task RequestReply(IMessageContext bus)
        {
            var pong = await bus.Request<PongMessage>(new PingMessage());
            // do something with the pong
        }
        // ENDSAMPLE

        // SAMPLE: CustomizedRequestReply
        public async Task CustomizedRequestReply(IMessageContext bus)
        {
            var pong = await bus.Request<PongMessage>(
                new PingMessage(),

                // Override the timeout period for the expected reply
                20.Seconds(),

                // Override the destination where the request is to be sent
                e => e.Destination = new Uri("tcp://someserver:2000")
            );


            // do something with the pong
        }
        // ENDSAMPLE
    }




    // SAMPLE: StaticRoutingApp
    public class StaticRoutingApp : JasperRegistry
    {
        public StaticRoutingApp(AppSettings settings)
        {
            // Explicitly add a single message type
            Publish.Message<PingMessage>()
                .To(settings.Transactions);

            // Publish any types matching the supplied filter
            // to this channel
            Publish.MessagesMatching("Message suffix", type => type.Name.EndsWith("Message"))
                .To(settings.Transactions);

            // Publish any message type contained in the assembly
            // to this channel, by supplying a type contained
            // within that assembly
            Publish.MessagesFromAssemblyContaining<PingMessage>()
                .To(settings.Transactions);

            // Publish any message type contained in the named
            // assembly to this channel
            Publish.MessagesFromAssembly(Assembly.Load(new AssemblyName("MyMessageLibrary")))
                .To(settings.Transactions);

            // Publish any message type contained in the
            // namespace given to this channel
            Publish.MessagesFromNamespace("MyMessageLibrary")
                .To(settings.Transactions);

            // Publish any message type contained in the namespace
            // of the type to this channel
            Publish.MessagesFromNamespaceContaining<PingMessage>()
                .To(settings.Transactions);
        }
    }
    // ENDSAMPLE
}

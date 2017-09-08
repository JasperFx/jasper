using System;
using System.Reflection;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Util;

namespace Jasper.Testing.Bus.Samples
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
            Messaging.Send<PingMessage>()
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
        public async Task SendPingsAndPongs(IServiceBus bus)
        {
            // Publish a message
            await bus.Send(new PingMessage());

            // Request/Reply
            var pong = await bus.Request<PongMessage>(new PingMessage());
        }
    }
    // ENDSAMPLE

    // SAMPLE: StaticRoutingApp
    public class StaticRoutingApp : JasperRegistry
    {
        public StaticRoutingApp(AppSettings settings)
        {
            // Explicitly add a single message type
            Messaging.Send<PingMessage>()
                .To(settings.Transactions);

            // Publish any types matching the supplied filter
            // to this channel
            Messaging.SendMatching("Message suffix", type => type.Name.EndsWith("Message"))
                .To(settings.Transactions);

            // Publish any message type contained in the assembly
            // to this channel, by supplying a type contained
            // within that assembly
            Messaging.SendFromAssemblyContaining<PingMessage>()
                .To(settings.Transactions);

            // Publish any message type contained in the named
            // assembly to this channel
            Messaging.SendFromAssembly(Assembly.Load(new AssemblyName("MyMessageLibrary")))
                .To(settings.Transactions);

            // Publish any message type contained in the
            // namespace given to this channel
            Messaging.SendFromNamespace("MyMessageLibrary")
                .To(settings.Transactions);

            // Publish any message type contained in the namespace
            // of the type to this channel
            Messaging.SendFromNamespaceContaining<PingMessage>()
                .To(settings.Transactions);
        }
    }
    // ENDSAMPLE
}

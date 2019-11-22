using Baseline.Dates;
using Bootstrapping.Configuration2;
using Jasper.Configuration;
using Jasper.Messaging.ErrorHandling;
using Microsoft.Extensions.Hosting;
using TestMessages;

namespace Jasper.Testing.Samples
{
    // SAMPLE: configuring-messaging-with-JasperOptions
    public class MyMessagingApp : JasperOptions
    {
        public MyMessagingApp()
        {
            // Configure handler policies
            Handlers.Retries.MaximumAttempts = 3;
            Handlers.Retries.Add(x => x.Handle<SqlException>().Reschedule(3.Seconds()));

            // Declare published messages
            Publish.Message<Message1>()
                .ToServerAndPort("server1", 2222);


            // Configure the built in transports
            Transports.LightweightListenerAt(2233);
        }
    }
    // ENDSAMPLE


    // SAMPLE: MyListeningApp
    public class MyListeningApp : JasperOptions
    {
        public MyListeningApp()
        {
            // Use the simpler, but transport specific syntax
            // to just declare what port the transport should use
            // to listen for incoming messages
            Transports.LightweightListenerAt(2233);
        }
    }
    // ENDSAMPLE


    // SAMPLE: LightweightTransportApp
    public class LightweightTransportApp : JasperOptions
    {
        public LightweightTransportApp()
        {
            // Set up a listener (this is optional)
            Transports.LightweightListenerAt(4000);

            // Publish the message Message2 to the DNS entry "remoteserver"
            Publish.Message<Message2>().ToServerAndPort("remoteserver", 2201);
        }
    }
    // ENDSAMPLE

    // SAMPLE: DurableTransportApp
    public class DurableTransportApp : JasperOptions
    {
        public DurableTransportApp()
        {
            // Set up a listener (this is optional)
            Transports.DurableListenerAt(2200);

            // With the RabbitMQ transport
            Transports.ListenForMessagesFrom("rabbitmq://server1/durable/queue1");
        }
    }
    // ENDSAMPLE


    // SAMPLE: LocalTransportApp
    public class LocalTransportApp : JasperOptions
    {
        public LocalTransportApp()
        {
            // Publish the message Message2 the important queue
            Publish.Message<Message2>()
                .ToLocalQueue("important");
        }
    }

    // ENDSAMPLE


    public class Samples
    {
        public void Go()
        {
            // SAMPLE: using-configuration-with-jasperoptions
            var host = Host.CreateDefaultBuilder()
                .UseJasper()
                .Start();

            // ENDSAMPLE
        }

    }
}

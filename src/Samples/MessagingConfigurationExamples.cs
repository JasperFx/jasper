using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Baseline.Dates;
using Bootstrapping.Configuration2;
using Jasper.Messaging.ErrorHandling;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using TestMessages;

namespace Jasper.Testing.Samples
{
    // SAMPLE: configuring-messaging-with-JasperRegistry
    public class MyMessagingApp : JasperRegistry
    {
        public MyMessagingApp()
        {
            // Configure handler policies
            Handlers.Retries.MaximumAttempts = 3;
            Handlers.Retries.Add(x => x.Handle<SqlException>().Reschedule(3.Seconds()));

            // Declare published messages
            Publish.Message<Message1>().To("tcp://server1:2222");

            // Configure the built in transports
            Transports.LightweightListenerAt(2233);
        }
    }
    // ENDSAMPLE


    // SAMPLE: MyListeningApp
    public class MyListeningApp : JasperRegistry
    {
        public MyListeningApp()
        {
            // Use the simpler, but transport specific syntax
            // to just declare what port the transport should use
            // to listen for incoming messages
            Transports.LightweightListenerAt(2233);

            // or use a Uri to declare both the transport type and port
            Transports.ListenForMessagesFrom("tcp://localhost:2233");
        }
    }
    // ENDSAMPLE




    // SAMPLE: LightweightTransportApp
    public class LightweightTransportApp : JasperRegistry
    {
        public LightweightTransportApp()
        {
            // Set up a listener (this is optional)
            Transports.LightweightListenerAt(4000);

            // Or do the exact same thing by supplying a Uri
            Transports.ListenForMessagesFrom("tcp://localhost:4000");

            // Publish the message Message2 to the DNS entry "remoteserver"
            Publish.Message<Message2>().To("tcp://remoteserver:2201");
        }
    }
    // ENDSAMPLE

    // SAMPLE: DurableTransportApp
    public class DurableTransportApp : JasperRegistry
    {
        public DurableTransportApp()
        {
            // Set up a listener (this is optional)
            Transports.DurableListenerAt(2200);

            // Or, alternatively set up durable listening by Uri
            Transports.ListenForMessagesFrom("tcp://localhost:2200/durable");

            // Or, alternatively set up durable listening by Uri
            Transports.ListenForMessagesFrom("durable://localhost:2200");

            // With the RabbitMQ transport
            Transports.ListenForMessagesFrom("rabbitmq://server1/durable/queue1");
        }
    }
    // ENDSAMPLE


    // SAMPLE: LoopbackTransportApp
    public class LoopbackTransportApp : JasperRegistry
    {
        public LoopbackTransportApp()
        {
            Handlers.Worker("important")
                .IsDurable()
                .MaximumParallelization(10);

            // Publish the message Message2 the important queue
            Publish.Message<Message2>().To("loopback://important");
        }
    }

    // ENDSAMPLE



    public class Samples
    {
        public void Go()
        {

            // SAMPLE: using-configuration-with-jasperoptions
            var host = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .UseJasper()
                .Start();

            // ENDSAMPLE
        }

        // SAMPLE: ConfigUsingApp
        public class ConfigUsingApp : JasperRegistry
        {
            public ConfigUsingApp()
            {
                Publish.Message<Message1>().ToUriValueInConfig("outgoing");

                // or

                Publish.AllMessagesToUriValueInConfig("outgoing");

                Transports.ListenForMessagesFromUriValueInConfig("listener");
            }
        }
        // ENDSAMPLE
    }
}

using System;
using System.Collections.Generic;
using Baseline.Dates;
using Jasper.Messaging.ErrorHandling;
using Jasper.Testing.Http;
using Jasper.Testing.Messaging.Samples;
using Jasper.Util;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

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
                .UseJasper(configure: (context, options) =>
                {
                    // I'm not using it here, but you have access to
                    // the ASP.Net Core HostingEnvironment
                    var hosting = context.HostingEnvironment;

                    // And the IConfiguration for your system
                    var config = context.Configuration;

                    // Add a transport listener at the Uri in
                    // your configuration
                    options.ListenForMessagesFrom(config["listener"]);

                    // Add a subscription for a specific message type
                    options.AddSubscription(Subscription.ForType<Message1>(config["outgoing"]));

                    // Or add a subscription for all messages
                    options.AddSubscription(Subscription.All(config["outgoing"]));
                })
                .Start();

            // ENDSAMPLE
        }

        // SAMPLE: ConfigUsingApp
        public class ConfigUsingApp : JasperRegistry
        {
            public ConfigUsingApp()
            {
                Settings.Messaging((context, options) =>
                {
                    // I'm not using it here, but you have access to
                    // the ASP.Net Core HostingEnvironment
                    var hosting = context.HostingEnvironment;

                    // And the IConfiguration for your system
                    var config = context.Configuration;

                    // Add a transport listener at the Uri in
                    // your configuration
                    options.ListenForMessagesFrom(config["listener"]);

                    // Add a subscription for a specific message type
                    options.AddSubscription(Subscription.ForType<Message1>(config["outgoing"]));

                    // Or add a subscription for all messages
                    options.AddSubscription(Subscription.All(config["outgoing"]));
                });
            }
        }
        // ENDSAMPLE
    }
}

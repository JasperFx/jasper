using System;
using Jasper.Messaging.Runtime;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.AzureServiceBus.Tests
{
    // SAMPLE: AppWithAzureServiceBus
    public class AppWithAzureServiceBus : JasperRegistry
    {
        public AppWithAzureServiceBus()
        {
            Include<AzureServiceBusTransportExtension>();
        }
    }
    // ENDSAMPLE

    public class Samples
    {
        public void hard_code_it()
        {
        }


        /*
        // SAMPLE: Main-activation-of-asb-with-startup
        public static int Main(string[] args)
        {
            return JasperHost.CreateDefaultBuilder()
                .UseJasper()
                .UseStartup<Startup>()
                .RunJasper(args);
        }
        // ENDSAMPLE
        */
    }


    // SAMPLE: asb-MessageSpecificTopicRoutingApp
    public class MessageSpecificTopicRoutingApp : JasperRegistry
    {
        public MessageSpecificTopicRoutingApp()
        {
            // Publish all messages to Azure Service Bus using the configured connection
            // string named 'azure' and use the message type name as the published
            // topic name
            Publish.AllMessagesTo("azureservicebus://azure/topic/*");


            // Make a subscription to all topic names that match known, handled message types
            // in this application using the configured connection string 'azure' and the subscription
            // 'appname'
            Transports.ListenForMessagesFrom("azureservicebus://azure/subscription/appname/topic/*");
        }
    }
    // ENDSAMPLE

    // SAMPLE: HardCodedASBConnection
    public class HardCodedConnectionApp : JasperRegistry
    {
        public HardCodedConnectionApp()
        {
            Settings
                .AddAzureServiceBusConnection("azure", "some connection string");

            Publish.AllMessagesTo("azureservicebus://azure/queue/outgoing");

            Transports.ListenForMessagesFrom("azureservicebus://azure/queue/incoming");
        }
    }
    // ENDSAMPLE

    // SAMPLE: Main-activation-of-asb-Startup
    public class Startup
    {
        // Both AzureServiceBusOptions and JasperOptions are registered services in your
        // Jasper application's IoC container, so are available to be injected into Startup.Configure()
        public void Configure(IConfiguration config, AzureServiceBusOptions asb,
            JasperOptions jasper)
        {
            // This code will add a new Azure Service Bus connection named "azure"
            asb.Connections.Add("azure", config["AzureServiceBusConnectionString"]);

            // Listen for messages from the 'queue1' queue at the connection string
            // configured above
            jasper.ListenForMessagesFrom("azureservicebus://azure/queue/queue1");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // add service registrations
        }
    }
    // ENDSAMPLE

    public class MySpecialProtocol : IAzureServiceBusProtocol
    {
        public Message WriteFromEnvelope(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public Envelope ReadEnvelope(Message message)
        {
            throw new NotImplementedException();
        }
    }

    // SAMPLE: CustomizedAzureServiceBusApp
    public class CustomizedAzureServiceBusApp : JasperRegistry
    {
        public CustomizedAzureServiceBusApp()
        {
            // There is another overload that will give you access
            // to the application's IHostingEnvironment and IConfiguration as well
            // if you prefer putting this into the JasperRegistry rather than
            // using Startup.Configure()
            Settings.ConfigureAzureServiceBus(options =>
            {
                // Configure a single endpoint
                options.ConfigureEndpoint("azureservicebus://azure/queue/incoming", endpoint =>
                {
                    // modify the endpoint
                });

                // Configure all the endpoints related to a specific connection string
                options.ConfigureEndpoint("azure", endpoint =>
                {
                    endpoint.ReceiveMode = ReceiveMode.ReceiveAndDelete;
                    endpoint.RetryPolicy = RetryPolicy.NoRetry;
                    endpoint.TokenProvider = new ManagedServiceIdentityTokenProvider();
                    endpoint.TransportType = TransportType.AmqpWebSockets;


                    // Override the envelope to message mapping for usage with non-Jasper applications
                    endpoint.Protocol = new MySpecialProtocol();
                });
            });
        }
    }

    // Or with Startup

    public class CustomizedStartup
    {
        public void Configure(AzureServiceBusOptions options)
        {
            // Configure a single endpoint
            options.ConfigureEndpoint("azureservicebus://azure/queue/incoming", endpoint =>
            {
                // modify the endpoint
            });

            // Configure all the endpoints related to a specific connection string
            options.ConfigureEndpoint("azure", endpoint =>
            {
                endpoint.ReceiveMode = ReceiveMode.ReceiveAndDelete;
                endpoint.RetryPolicy = RetryPolicy.NoRetry;
                endpoint.TokenProvider = new ManagedServiceIdentityTokenProvider();
                endpoint.TransportType = TransportType.AmqpWebSockets;
            });
        }
    }

    // ENDSAMPLE
}

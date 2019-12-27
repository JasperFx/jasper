using System.Net;
using Jasper;
using Jasper.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace IntegrationTests.Samples
{
    // SAMPLE: AppWithRabbitMq
    public class AppWithRabbitMq : JasperOptions
    {
        public AppWithRabbitMq()
        {
            // This explicitly registers the Rabbit MQ transport
            //Include<Jasper.RabbitMQ.RabbitMqTransportExtension>();
        }
    }
    // ENDSAMPLE

    public class Samples
    {
        public void hard_code_it()
        {

        }



        /*
        // SAMPLE: Main-activation-of-rabbit-with-startup
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



    // SAMPLE: rabbit-MessageSpecificTopicRoutingApp
    public class MessageSpecificTopicRoutingApp : JasperOptions
    {
        public MessageSpecificTopicRoutingApp()
        {
//            // Publish all messages to Rabbit Mq using the configured connection
//            // string named 'rabbit' and use the message type name as the published
//            // topic name
//            Publish.AllMessagesTo("rabbitmq://rabbit/topic/*");
//
//
//            // Make a subscription to all topic names that match known, handled message types
//            // in this application using the configured connection string 'rabbit'.
//            Transports.ListenForMessagesFrom("rabbitmq://rabbit/topic/*");
        }
    }
    // ENDSAMPLE

    // SAMPLE: HardCodedRabbitConnection
    public class HardCodedConnectionApp : JasperOptions
    {
        public HardCodedConnectionApp()
        {
            // This adds a connection string to the host "rabbitserver" using
            // the default port
//            Settings
//                .AddRabbitMqHost("rabbitserver");
//
//            Publish.AllMessagesTo("rabbitmq://rabbitserver/queue/outgoing");
//
//            Transports.ListenForMessagesFrom("rabbitmq://rabbitserver/queue/incoming");
        }
    }
    // ENDSAMPLE

    // SAMPLE: Main-activation-of-rabbit-Startup
    public class Startup
    {
        // Both RabbitMqOptions and JasperOptions are registered services in your
        // Jasper application's IoC container, so are available to be injected into Startup.Configure()
//        public void Configure(IConfiguration config, RabbitMqOptions rabbit, JasperOptions jasper)
//        {
//            // This code will add a new Rabbit MQ connection named "rabbit"
//            rabbit.Connections.Add("rabbit", config["RabbitMqConnectionString"]);
//
//            // Listen for messages from the 'queue1' queue at the connection string
//            // configured above
//            jasper.Transports.ListenForMessagesFrom("rabbitmq://rabbit/queue/queue1");
//        }

        public void ConfigureServices(IServiceCollection services)
        {
            // add service registrations
        }
    }
    // ENDSAMPLE

    public class MySpecialProtocol : IRabbitMqProtocol
    {
        public void WriteFromEnvelope(Envelope envelope, IBasicProperties properties)
        {
            throw new System.NotImplementedException();
        }

        public Envelope ReadEnvelope(byte[] body, IBasicProperties properties)
        {
            throw new System.NotImplementedException();
        }
    }

    // SAMPLE: CustomizedRabbitApp
    public class CustomizedRabbitMqApp : JasperOptions
    {
        public CustomizedRabbitMqApp()
        {
            // There is another overload that will give you access
            // to the application's IHostingEnvironment and IConfiguration as well
            // if you prefer putting this into the JasperOptions rather than
            // using Startup.Configure()
//            Settings.ConfigureRabbitMq((options, hosting, config) =>
//            {
//                // Configure a single endpoint
//                options.ConfigureEndpoint("rabbitmq://rabbit/queue/incoming", endpoint =>
//                {
//                    // modify the endpoint
//                });
//
//                // Configure all the endpoints related to a specific connection string
//                options.ConfigureEndpoint("rabbit", endpoint =>
//                {
//                    // Customize the Rabbit MQ ConnectionFactory
//                    endpoint.ConnectionFactory.UserName = config["rabbit.user"];
//                    endpoint.ConnectionFactory.Password = config["rabbit.password"];
//
//
//                    // Override the envelope to message mapping for usage with non-Jasper applications
//                    endpoint.Protocol = new MySpecialProtocol();
//                });
//            });
        }
    }

    // Or with Startup

    public class CustomizedStartup
    {
//        public void Configure(RabbitMqOptions options, IConfiguration config)
//        {
//            // Configure a single endpoint
//            options.ConfigureEndpoint("rabbitmq://rabbit/queue/incoming", endpoint =>
//            {
//                // modify the endpoint
//            });
//
//            // Configure all the endpoints related to a specific connection string
//            options.ConfigureEndpoint("rabbit", endpoint =>
//            {
//                // Customize the Rabbit MQ ConnectionFactory
//                endpoint.ConnectionFactory.UserName = config["rabbit.user"];
//                endpoint.ConnectionFactory.Password = config["rabbit.password"];
//
//
//                // Override the envelope to message mapping for usage with non-Jasper applications
//                endpoint.Protocol = new MySpecialProtocol();
//            });
//        }
    }
    // ENDSAMPLE
}

using Baseline.Dates;
using Bootstrapping.Configuration2;
using Jasper.Configuration;
using Jasper.ErrorHandling;
using TestMessages;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
using Host = Microsoft.AspNetCore.WebHost;
#else
using Microsoft.Extensions.Hosting;
#endif

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
            Endpoints.Publish(x =>
            {
                x.Message<Message1>();
                x.ToServerAndPort("server1", 2222);
            });

            // Configure the built in transports
            Endpoints.ListenAtPort(2233);
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
            Endpoints.ListenAtPort(2233);
        }
    }
    // ENDSAMPLE


    // SAMPLE: LightweightTransportApp
    public class LightweightTransportApp : JasperOptions
    {
        public LightweightTransportApp()
        {
            // Set up a listener (this is optional)
            Endpoints.ListenAtPort(4000);

            Endpoints.Publish(x =>
            {
                x.Message<Message2>()
                    .ToServerAndPort("remoteserver", 2201);
            });
        }
    }
    // ENDSAMPLE

    // SAMPLE: DurableTransportApp
    public class DurableTransportApp : JasperOptions
    {
        public DurableTransportApp()
        {
            Endpoints
                .PublishAllMessages()
                .ToServerAndPort("server1", 2201)

                // This applies the store and forward persistence
                // to the outgoing message
                .Durably();

            // Set up a listener (this is optional)
            Endpoints.ListenAtPort(2200)

                // This applies the message persistence
                // to the incoming endpoint such that incoming
                // messages are first saved to the application
                // database before attempting to handle the
                // incoming message
                .Durable();

        }
    }
    // ENDSAMPLE


    // SAMPLE: LocalTransportApp
    public class LocalTransportApp : JasperOptions
    {
        public LocalTransportApp()
        {
            // Publish the message Message2 the "important"
            // local queue
            Endpoints.Publish(x =>
            {
                x.Message<Message2>();
                x.ToLocalQueue("important");
            });
        }
    }

    // ENDSAMPLE

    // SAMPLE: LocalDurableTransportApp
    public class LocalDurableTransportApp : JasperOptions
    {
        public LocalDurableTransportApp()
        {
            // Make the default local queue durable
            Endpoints.DefaultLocalQueue.Durable();

            // Or do just this by name
            Endpoints
                .LocalQueue("important")
                .Durable();
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

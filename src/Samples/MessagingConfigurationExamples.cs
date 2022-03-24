using Baseline.Dates;
using Jasper;
using Jasper.ErrorHandling;
using Jasper.Tcp;
using Microsoft.Extensions.Hosting;
using TestMessages;

namespace Samples
{
    #region sample_configuring_messaging_with_JasperOptions
    public class MyMessagingApp : JasperOptions
    {
        public MyMessagingApp()
        {
            // Configure handler policies
            Handlers
                .OnException<SqlException>()
                .RetryLater(3.Seconds());

            // Declare published messages
            Publish(x =>
            {
                x.Message<Message1>();
                x.ToServerAndPort("server1", 2222);
            });

            // Configure the built in transports
            this.ListenAtPort(2233);
        }
    }
    #endregion


    #region sample_MyListeningApp
    public class MyListeningApp : JasperOptions
    {
        public MyListeningApp()
        {
            // Use the simpler, but transport specific syntax
            // to just declare what port the transport should use
            // to listen for incoming messages
            this.ListenAtPort(2233);
        }
    }
    #endregion


    #region sample_LightweightTransportApp
    public class LightweightTransportApp : JasperOptions
    {
        public LightweightTransportApp()
        {
            // Set up a listener (this is optional)
            this.ListenAtPort(4000);

            Publish(x =>
            {
                x.Message<Message2>()
                    .ToServerAndPort("remoteserver", 2201);
            });
        }
    }
    #endregion

    #region sample_DurableTransportApp
    public class DurableTransportApp : JasperOptions
    {
        public DurableTransportApp()
        {
            PublishAllMessages()
                .ToServerAndPort("server1", 2201)

                // This applies the store and forward persistence
                // to the outgoing message
                .Durably();

            // Set up a listener (this is optional)
            this.ListenAtPort(2200)

                // This applies the message persistence
                // to the incoming endpoint such that incoming
                // messages are first saved to the application
                // database before attempting to handle the
                // incoming message
                .DurablyPersistedLocally();

        }
    }
    #endregion


    #region sample_LocalTransportApp
    public class LocalTransportApp : JasperOptions
    {
        public LocalTransportApp()
        {
            // Publish the message Message2 the "important"
            // local queue
            Publish(x =>
            {
                x.Message<Message2>();
                x.ToLocalQueue("important");
            });
        }
    }

    #endregion

    #region sample_LocalDurableTransportApp
    public class LocalDurableTransportApp : JasperOptions
    {
        public LocalDurableTransportApp()
        {
            // Make the default local queue durable
            DefaultLocalQueue.DurablyPersistedLocally();

            // Or do just this by name
            LocalQueue("important")
                .DurablyPersistedLocally();
        }
    }

    #endregion


    public class Samples
    {
        public void Go()
        {
            #region sample_using_configuration_with_jasperoptions
            var host = Host.CreateDefaultBuilder()
                .UseJasper()
                .Start();

            #endregion
        }

    }
}

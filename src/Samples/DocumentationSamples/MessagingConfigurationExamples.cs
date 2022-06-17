using Baseline.Dates;
using Jasper;
using Jasper.ErrorHandling;
using Jasper.Transports.Tcp;
using Microsoft.Extensions.Hosting;
using TestMessages;

namespace DocumentationSamples
{
    public static class MessagingConfigurationExamples
    {
        public static async Task configuring_messaging_with_JasperOptions()
        {
            #region sample_configuring_messaging_with_JasperOptions

            using var host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    // Configure handler policies
                    opts.Handlers
                        .OnException<SqlException>()
                        .ScheduleRetry(3.Seconds());

                    // Declare published messages
                    opts.Publish(x =>
                    {
                        x.Message<Message1>();
                        x.ToServerAndPort("server1", 2222);
                    });

                    // Configure the built in transports
                    opts.ListenAtPort(2233);
                }).StartAsync();

            #endregion
        }

        public static async Task MyListeningApp()
        {
            #region sample_MyListeningApp

            using var host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    // Use the simpler, but transport specific syntax
                    // to just declare what port the transport should use
                    // to listen for incoming messages
                    opts.ListenAtPort(2233);
                }).StartAsync();

            #endregion
        }

        public static async Task LightweightTransportApp()
        {
            #region sample_LightweightTransportApp

            using var host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    // Set up a listener (this is optional)
                    opts.ListenAtPort(4000);

                    opts.Publish(x =>
                    {
                        x.Message<Message2>()
                            .ToServerAndPort("remoteserver", 2201);
                    });
                }).StartAsync();

            #endregion
        }

        public static async Task DurableTransportApp()
        {
            #region sample_DurableTransportApp

            using var host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.PublishAllMessages()
                        .ToServerAndPort("server1", 2201)

                        // This applies the store and forward persistence
                        // to the outgoing message
                        .Durably();

                    // Set up a listener (this is optional)
                    opts.ListenAtPort(2200)

                        // This applies the message persistence
                        // to the incoming endpoint such that incoming
                        // messages are first saved to the application
                        // database before attempting to handle the
                        // incoming message
                        .DurablyPersistedLocally();
                }).StartAsync();

            #endregion
        }

        public static async Task LocalTransportApp()
        {
            #region sample_LocalTransportApp

            using var host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    // Publish Message2 messages to the "important"
                    // local queue
                    opts.PublishMessage<Message2>()
                        .ToLocalQueue("important");
                }).StartAsync();

            #endregion
        }
    }


    public class Samples
    {
        public static async Task LocalDurableTransportApp()
        {
            #region sample_LocalDurableTransportApp

            using var host = await Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    // Make the default local queue durable
                    opts.DefaultLocalQueue.DurablyPersistedLocally();

                    // Or do just this by name
                    opts.LocalQueue("important")
                        .DurablyPersistedLocally();
                }).StartAsync();

            #endregion
        }

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

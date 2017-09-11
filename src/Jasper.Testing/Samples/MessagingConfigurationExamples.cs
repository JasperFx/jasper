using System;
using System.Collections.Generic;
using Baseline;
using Baseline.Dates;
using Jasper.Bus.ErrorHandling;
using Jasper.Testing.Bus.Runtime;
using Jasper.Testing.Bus.Samples;
using Jasper.Util;
using Microsoft.Extensions.Configuration;

namespace Jasper.Testing.Samples
{
    // SAMPLE: configuring-messaging-with-JasperRegistry
    public class MyMessagingApp : JasperRegistry
    {
        public MyMessagingApp()
        {
            // Configure handler policies
            Handlers.DefaultMaximumAttempts = 3;
            Handlers.OnException<SqlException>().RetryLater(3.Seconds());

            // Declare published messages
            Publish.Message<Message1>().To("tcp://server1:2222");

            // Register to receive messages
            Subscribe.At("tcp://loadbalancer1:2233");
            Subscribe.To<Message2>();
            Subscribe.To(type => type.IsInNamespace("MyMessagingApp.Incoming"));

            // Configure the built in transports
            Transports.Lightweight.ListenOnPort(2233);
            Transports.Durable.Disable();
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
            Transports.Lightweight.ListenOnPort(2233);

            // or use a Uri to declare both the transport type and port
            Transports.ListenForMessagesFrom("tcp://localhost:2233");
        }
    }
    // ENDSAMPLE


    // SAMPLE: configuring-bus-application-with-settings
    public class MySettings
    {
        public int LightweightPort { get; set; } = 2222;
        public Uri DefaultListener { get; set; } = "durable://localhost:2223".ToUri();

        public int MaximumSendAttempts { get; set; } = 5;
    }

    public class MyAppWithSettings : JasperRegistry
    {
        public MyAppWithSettings()
        {
            Configuration
                .AddEnvironmentVariables()
                .AddJsonFile("appSettings.json");

            Settings.With<MySettings>(_ =>
            {
                Transports.Lightweight
                    .ListenOnPort(_.LightweightPort)
                    .MaximumSendAttempts(_.MaximumSendAttempts);

                Transports.ListenForMessagesFrom(_.DefaultListener);

            });
        }
    }
    // ENDSAMPLE


    // SAMPLE: configuring-via-uri-lookup
    public class MyAppUsingUriLookups : JasperRegistry
    {
        public MyAppUsingUriLookups()
        {
            Configuration
                .AddInMemoryCollection(new Dictionary<string, string>{{"incoming", "tcp://server3:2000"}});

            // This usage assumes that there is a value in the configuration
            // with the key "incoming" that corresponds to a Uri
            Transports.ListenForMessagesFrom("config://incoming");
        }
    }
    // ENDSAMPLE
}

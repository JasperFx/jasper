using System;
using System.Collections.Generic;
using System.Linq;
using Jasper;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Stub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestMessages;
using Xunit;

namespace MessagingTests.Samples
{

    // SAMPLE: StubTransport-IntegrationFixture
    public class IntegrationFixture : IDisposable
    {
        public IntegrationFixture()
        {
            // Bootstrap your application with a few overrides to
            // configuration, and slightly modify the Jasper configuration
            JasperHost = Host.CreateDefaultBuilder()
                .OverrideConfigValue("outgoing", "stub://outgoing")
                .OverrideConfigValue("incoming", "stub://incoming")
                .UseJasper<MyJasperApp>()

                // This gives you an IJasperHost, but this works as well with an
                // IWebHost as well
                .StartJasper();
        }

        public IJasperHost JasperHost { get; }

        public void Dispose()
        {
            JasperHost?.Dispose();
        }
    }
    // ENDSAMPLE

    // SAMPLE: StubTransport-IntegrationContext
    public class IntegrationContext : IClassFixture<IntegrationFixture>
    {
        public IntegrationContext(IntegrationFixture fixture)
        {
            Host = fixture.JasperHost;

            // Clean out any previously received messages
            // You'll likely want to use this before any test
            // executes
            Host.ClearStubTransportSentList();
        }

        public IJasperHost Host { get; }
    }
    // ENDSAMPLE

    // SAMPLE: StubTransport-test-fixture-class
    public class when_doing_some_kind_of_action : IntegrationContext
    {
        public when_doing_some_kind_of_action(IntegrationFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void check_that_a_certain_message_was_sent()
        {
            // Perform some kind of action against your system
            // that should result in a message being published

            // This extension method retrieves all the known
            // Envelopes that have been published through
            // the application
            var envelopes = Host.GetAllEnvelopesSent();

            // If you expect there to be a single message of
            // type Message1, you can find -- and verify there
            // was only one -- with this code
            var message = envelopes
                .Select(x => x.Message)
                .OfType<Message1>()
                .FirstOrDefault();

            // then do assertions on the message variable
        }
    }
    // ENDSAMPLE

    // SAMPLE: StubTransport-MyJasperApp
    public class MyJasperApp : JasperRegistry
    {
        public MyJasperApp()
        {
            // This app has a single outbound publishing Uri that
            // is expected to be in configuration
            Publish.AllMessagesToUriValueInConfig("outgoing");


            Transports.ListenForMessagesFromUriValueInConfig("incoming");
        }
    }
    // ENDSAMPLE
}

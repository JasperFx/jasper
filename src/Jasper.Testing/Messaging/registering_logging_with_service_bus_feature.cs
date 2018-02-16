using System;
using System.Linq;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Testing.Messaging.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class registering_logging_with_service_bus_feature : IntegrationContext
    {
        [Fact]
        public void no_console_logging_by_default()
        {
            withAllDefaults();

            Runtime.Container.ShouldNotHaveRegistration<IMessageEventSink, ConsoleMessageSink>();
        }

        [Fact]
        public void using_console_logging()
        {
            with(_ => _.Logging.UseConsoleLogging = true);

            Runtime.Container.ShouldHaveRegistration<IMessageEventSink, ConsoleMessageSink>();
        }

        [Fact]
        public void using_console_logging_has_exception_sing()
        {
            with(_ => _.Logging.UseConsoleLogging = true);

            Runtime.Container.ShouldHaveRegistration<IExceptionSink, ConsoleExceptionSink>();
        }

        [Fact]
        public void using_console_logging_transports()
        {
            with(_ => _.Logging.UseConsoleLogging = true);

            Runtime.Container.ShouldHaveRegistration<ITransportEventSink, ConsoleTransportSink>();
        }

        [Fact]
        public void explicitly_add_logger()
        {
            with(_ => _.Logging.LogMessageEventsWith<ConsoleMessageSink>());

            Runtime.Container.ShouldHaveRegistration<IMessageEventSink, ConsoleMessageSink>();
        }

        [Fact]
        public void explicitly_add_logger_transport()
        {
            with(_ => _.Logging.LogTransportEventsWith<ConsoleTransportSink>());

            Runtime.Container.ShouldHaveRegistration<ITransportEventSink, ConsoleTransportSink>();
        }

        [Fact]
        public void explicitly_add_logger_2()
        {
            with(_ => _.Logging.LogMessageEventsWith(new ConsoleMessageSink()));

            Runtime.Container.Model.For<IMessageEventSink>().Instances
                .Any(x => x.ImplementationType == typeof(ConsoleMessageSink))
                .ShouldBeTrue();
        }

        [Fact]
        public void explicitly_add_logger_2_transport()
        {
            with(_ => _.Logging.LogTransportEventsWith(new ConsoleTransportSink()));

            Runtime.Container.Model.For<ITransportEventSink>().Instances
                .Any(x => x.ImplementationType == typeof(ConsoleTransportSink))
                .ShouldBeTrue();
        }
    }

    // SAMPLE: SampleMessageLogger
    public class SampleMessageSink : MessageSinkBase
    {
        public override void Sent(Envelope envelope)
        {
            // do something with the information
        }

        public override void Received(Envelope envelope)
        {
            // do something with the information
        }
    }
    // ENDSAMPLE

    // SAMPLE: SampleTransportLogger
    public class SampleTransportSink : TransportSinkBase
    {
        public override void CircuitBroken(Uri destination)
        {
            // do something with the information
        }

        public override void CircuitResumed(Uri destination)
        {
            // do something with the information
        }
    }
    // ENDSAMPLE

    // SAMPLE: AppWithCustomLogging
    public class AppWithCustomLogging : JasperRegistry
    {
        public AppWithCustomLogging()
        {
            // Shorthand
            Logging.LogMessageEventsWith<SampleMessageSink>();
            Logging.LogTransportEventsWith<SampleTransportSink>();

            // Uglier equivalent
            Services.AddTransient<IMessageEventSink, SampleMessageSink>();
            Services.AddTransient<ITransportEventSink, SampleTransportSink>();
        }
    }
    // ENDSAMPLE

    // SAMPLE: UsingConsoleLoggingApp
    public class UsingConsoleLoggingApp : JasperRegistry
    {
        public UsingConsoleLoggingApp()
        {
            Logging.UseConsoleLogging = true;
        }
    }
    // ENDSAMPLE
}

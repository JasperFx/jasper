using System.Linq;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class registering_logging_with_service_bus_feature : IntegrationContext
    {
        [Fact]
        public void no_console_logging_by_default()
        {
            withAllDefaults();

            Runtime.Container.ShouldNotHaveRegistration<IMessageLogger, ConsoleMessageLogger>();
        }

        [Fact]
        public void using_console_logging()
        {
            with(_ => _.Logging.UseConsoleLogging = true);

            Runtime.Container.ShouldHaveRegistration<IMessageLogger, ConsoleMessageLogger>();
        }

        [Fact]
        public void using_console_logging_transports()
        {
            with(_ => _.Logging.UseConsoleLogging = true);

            Runtime.Container.ShouldHaveRegistration<ITransportLogger, ConsoleTransportLogger>();
        }

        [Fact]
        public void explicitly_add_logger()
        {
            with(_ => _.Logging.LogBusEventsWith<ConsoleMessageLogger>());

            Runtime.Container.ShouldHaveRegistration<IMessageLogger, ConsoleMessageLogger>();
        }

        [Fact]
        public void explicitly_add_logger_transport()
        {
            with(_ => _.Logging.LogTransportEventsWith<ConsoleTransportLogger>());

            Runtime.Container.ShouldHaveRegistration<ITransportLogger, ConsoleTransportLogger>();
        }

        [Fact]
        public void explicitly_add_logger_2()
        {
            with(_ => _.Logging.LogBusEventsWith(new ConsoleMessageLogger()));


            Runtime.Services
                .Any(x => x.ServiceType == typeof(IMessageLogger) && x.ImplementationInstance is ConsoleMessageLogger).ShouldBeTrue();
        }

        [Fact]
        public void explicitly_add_logger_2_transport()
        {
            with(_ => _.Logging.LogTransportEventsWith(new ConsoleTransportLogger()));


            Runtime.Services
                .Any(x => x.ServiceType == typeof(ITransportLogger) && x.ImplementationInstance is ConsoleTransportLogger).ShouldBeTrue();
        }
    }

    // SAMPLE: SampleMessageLogger
    public class SampleMessageLogger : MessageLoggerBase
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

    // SAMPLE: AppWithCustomLogging
    public class AppWithCustomLogging : JasperRegistry
    {
        public AppWithCustomLogging()
        {
            // Shorthand
            Logging.LogBusEventsWith<SampleMessageLogger>();

            // Uglier equivalent
            Services.AddTransient<IMessageLogger, SampleMessageLogger>();
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

using System.Linq;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Testing.AspNetCoreIntegration;
using Jasper.Testing.Bus.Bootstrapping;
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

            Runtime.ShouldNotHaveRegistration<IBusLogger, ConsoleBusLogger>();
        }

        [Fact]
        public void using_console_logging()
        {
            with(_ => _.Logging.UseConsoleLogging = true);

            Runtime.ShouldHaveRegistration<IBusLogger, ConsoleBusLogger>();
        }

        [Fact]
        public void explicitly_add_logger()
        {
            with(_ => _.Logging.LogBusEventsWith<ConsoleBusLogger>());

            Runtime.ShouldHaveRegistration<IBusLogger, ConsoleBusLogger>();
        }

        [Fact]
        public void explicitly_add_logger_2()
        {
            with(_ => _.Logging.LogBusEventsWith(new ConsoleBusLogger()));


            Runtime.Services
                .Any(x => x.ServiceType == typeof(IBusLogger) && x.ImplementationInstance is ConsoleBusLogger).ShouldBeTrue();
        }
    }
}

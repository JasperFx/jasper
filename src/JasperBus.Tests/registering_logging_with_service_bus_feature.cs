using System.Linq;
using JasperBus.Tests.Bootstrapping;
using Shouldly;
using Xunit;

namespace JasperBus.Tests
{
    public class registering_logging_with_service_bus_feature : IntegrationContext
    {
        [Fact]
        public void no_console_logging_by_default()
        {
            withAllDefaults();

            Runtime.Container.ShouldNotHaveRegistration<IBusLogger, ConsoleBusLogger>();
        }

        [Fact]
        public void using_console_logging()
        {
            with(_ => _.Logging.UseConsoleLogging = true);

            Runtime.Container.ShouldHaveRegistration<IBusLogger, ConsoleBusLogger>();
        }

        [Fact]
        public void explicitly_add_logger()
        {
            with(_ => _.Logging.LogBusEventsWith<ConsoleBusLogger>());

            Runtime.Container.ShouldHaveRegistration<IBusLogger, ConsoleBusLogger>();
        }

        [Fact]
        public void explicitly_add_logger_2()
        {
            with(_ => _.Logging.LogBusEventsWith(new ConsoleBusLogger()));

            Runtime.Container.GetAllInstances<IBusLogger>().OfType<ConsoleBusLogger>()
                .Any().ShouldBeTrue();
        }
    }
}
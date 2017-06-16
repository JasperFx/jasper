using System.Linq;
using Jasper.Bus;
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

            Bootstrapping.ContainerExtensions.ShouldNotHaveRegistration<IBusLogger, ConsoleBusLogger>(Runtime.Container);
        }

        [Fact]
        public void using_console_logging()
        {
            with(_ => _.Logging.UseConsoleLogging = true);

            Bootstrapping.ContainerExtensions.ShouldHaveRegistration<IBusLogger, ConsoleBusLogger>(Runtime.Container);
        }

        [Fact]
        public void explicitly_add_logger()
        {
            with(_ => _.Logging.LogBusEventsWith<ConsoleBusLogger>());

            Bootstrapping.ContainerExtensions.ShouldHaveRegistration<IBusLogger, ConsoleBusLogger>(Runtime.Container);
        }

        [Fact]
        public void explicitly_add_logger_2()
        {
            with(_ => _.Logging.LogBusEventsWith(new ConsoleBusLogger()));

            ShouldBeBooleanExtensions.ShouldBeTrue(Runtime.Container.GetAllInstances<IBusLogger>().OfType<ConsoleBusLogger>()
                    .Any());
        }
    }
}
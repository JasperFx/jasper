using Jasper.Bus;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class BusLoggerTester
    {
        [Fact]
        public void resolves_no_loggers_as_nullo()
        {
            BusLogger.Combine(new IBusLogger[0])
                .ShouldBeOfType<BusLogger>();
        }

        [Fact]
        public void resolves_single_logger_as_just_that_instance()
        {
            var inner = new ConsoleBusLogger();

            BusLogger.Combine(new IBusLogger[] {inner})
                .ShouldBeSameAs(inner);
        }

        [Fact]
        public void resolves_multiple_loggers_to_composite()
        {
            var inner1 = Substitute.For<IBusLogger>();
            var inner2 = Substitute.For<IBusLogger>();
            var inner3 = Substitute.For<IBusLogger>();

            BusLogger.Combine(new IBusLogger[] {inner1, inner2, inner3})
                .ShouldBeOfType<CompositeLogger>()
                .Loggers.ShouldHaveTheSameElementsAs(inner1, inner2, inner3);
        }
    }
}
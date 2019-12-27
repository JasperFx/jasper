using Jasper.Configuration;
using Jasper.Transports.Local;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class SenderConfigurationTests
    {
        [Fact]
        public void durably()
        {
            var endpoint = new LocalQueueSettings("foo");
            endpoint.IsDurable.ShouldBeFalse();

            var expression = new SubscriberConfiguration(endpoint);
            expression.Durably();

            endpoint.IsDurable.ShouldBeTrue();
        }

        [Fact]
        public void lightweight()
        {
            var endpoint = new LocalQueueSettings("foo");
            endpoint.IsDurable = true;

            var expression = new SubscriberConfiguration(endpoint);
            expression.Lightweight();

            endpoint.IsDurable.ShouldBeFalse();
        }
    }
}

using Jasper.Messaging.Transports.Local;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class ListenerConfigurationTests
    {
        [Fact]
        public void sets_is_listener()
        {
            var endpoint = new LocalQueueSettings("temp");
            endpoint.IsListener.ShouldBeFalse();

            new ListenerConfiguration(endpoint);

            endpoint.IsListener.ShouldBeTrue();
        }
    }
}

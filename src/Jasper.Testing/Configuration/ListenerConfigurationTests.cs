using Jasper.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class ListenerConfigurationTests
    {
        [Fact]
        public void sets_is_listener()
        {
            var endpoint = new Endpoint();
            endpoint.IsListener.ShouldBeFalse();

            new ListenerConfiguration(endpoint);

            endpoint.IsListener.ShouldBeTrue();
        }
    }
}

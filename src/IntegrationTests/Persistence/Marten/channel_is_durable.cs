using Jasper.Messaging;
using Jasper.Util;
using Servers;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests
{
    public class channel_is_durable : MartenContext
    {
        [Fact]
        public void channels_that_are_or_are_not_durable()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(MartenContainer.ConnectionString);
                _.Include<MartenBackedPersistence>();
            }))
            {
                var channels = runtime.Get<IChannelGraph>();
                channels.GetOrBuildChannel("loopback://one".ToUri()).IsDurable.ShouldBeFalse();
                channels.GetOrBuildChannel("loopback://durable/two".ToUri()).IsDurable.ShouldBeTrue();

                channels.GetOrBuildChannel("tcp://server1".ToUri()).IsDurable.ShouldBeFalse();
                channels.GetOrBuildChannel("tcp://server2/durable".ToUri()).IsDurable.ShouldBeTrue();
            }
        }

        public channel_is_durable(DockerFixture<MartenContainer> fixture) : base(fixture)
        {
        }
    }
}

using Jasper.Bus;
using Jasper.Marten.Tests.Setup;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests
{
    public class channel_is_durable
    {
        [Fact]
        public void channels_that_are_or_are_not_durable()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(ConnectionSource.ConnectionString);
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
    }
}

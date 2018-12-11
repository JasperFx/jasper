using Jasper;
using Jasper.Messaging;
using Jasper.Persistence.Marten;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.Marten
{
    public class channel_is_durable : MartenContext
    {
        [Fact]
        public void channels_that_are_or_are_not_durable()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(Servers.PostgresConnectionString);
                _.Include<MartenBackedPersistence>();
            }))
            {
                var channels = runtime.Get<ISubscriberGraph>();
                channels.GetOrBuild("loopback://one".ToUri()).IsDurable.ShouldBeFalse();
                channels.GetOrBuild("loopback://durable/two".ToUri()).IsDurable.ShouldBeTrue();

                channels.GetOrBuild("tcp://server1".ToUri()).IsDurable.ShouldBeFalse();
                channels.GetOrBuild("tcp://server2/durable".ToUri()).IsDurable.ShouldBeTrue();
            }
        }
    }
}

using IntegrationTests;
using Jasper.Messaging;
using Jasper.Persistence.Marten;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class channel_is_durable : PostgresqlContext
    {
        [Fact]
        public void channels_that_are_or_are_not_durable()
        {
            using (var runtime = JasperHost.For(_ =>
            {
                _.MartenConnectionStringIs(Servers.PostgresConnectionString);
                _.Include<MartenBackedPersistence>();
            }))
            {
                var channels = runtime.Get<IMessagingRoot>();
                channels.Runtime.GetOrBuildSendingAgent("local://one".ToUri()).IsDurable.ShouldBeFalse();
                channels.Runtime.GetOrBuildSendingAgent("local://durable/two".ToUri()).IsDurable.ShouldBeTrue();

                channels.Runtime.GetOrBuildSendingAgent("tcp://server1".ToUri()).IsDurable.ShouldBeFalse();
                channels.Runtime.GetOrBuildSendingAgent("tcp://server2/durable".ToUri()).IsDurable.ShouldBeTrue();
            }
        }
    }
}

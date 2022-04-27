using IntegrationTests;
using Jasper.Persistence.Marten;
using Jasper.Runtime;
using Jasper.Util;
using Marten;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class channel_is_durable : PostgresqlContext
    {
        [Fact]
        public void channels_that_are_or_are_not_durable()
        {
            using var host = JasperHost.For(opts =>
            {
                opts.Services.AddMarten(Servers.PostgresConnectionString)
                    .IntegrateWithJasper();
            });

            var runtime = host.Get<IJasperRuntime>();
            runtime.Endpoints.GetOrBuildSendingAgent("local://one".ToUri()).IsDurable.ShouldBeFalse();
            runtime.Endpoints.GetOrBuildSendingAgent("local://durable/two".ToUri()).IsDurable.ShouldBeTrue();

            runtime.Endpoints.GetOrBuildSendingAgent("tcp://server1:2000".ToUri()).IsDurable.ShouldBeFalse();
            runtime.Endpoints.GetOrBuildSendingAgent("tcp://server2:3000/durable".ToUri()).IsDurable.ShouldBeTrue();
        }
    }
}

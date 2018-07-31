using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Consul;
using Jasper.Consul.Internal;
using Servers;
using Xunit;

namespace IntegrationTests.Consul
{
    [Collection("consul")]
    public class ConsulContext : IClassFixture<DockerFixture<ConsulContainer>>
    {
        public ConsulContext(DockerFixture<ConsulContainer> fixture)
        {
        }
    }
}

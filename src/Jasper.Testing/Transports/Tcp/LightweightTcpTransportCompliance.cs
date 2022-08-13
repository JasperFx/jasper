using System.Threading.Tasks;
using Jasper.Transports.Tcp;
using Jasper.Util;
using TestingSupport;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Testing.Transports.Tcp
{
    public class LightweightTcpFixture : SendingComplianceFixture, IAsyncLifetime
    {
        public LightweightTcpFixture() : base($"tcp://localhost:{PortFinder.GetAvailablePort()}/incoming".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            await SenderIs(opts =>
            {
                opts.ListenAtPort(PortFinder.GetAvailablePort());
            });

            await ReceiverIs(opts =>
            {
                opts.ListenAtPort(OutboundAddress.Port);
            });
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }


    [Collection("compliance")]
    public class LightweightTcpTransportCompliance : SendingCompliance<LightweightTcpFixture>
    {

    }


}

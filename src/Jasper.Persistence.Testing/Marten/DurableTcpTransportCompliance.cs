using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Jasper.Util;
using Marten;
using TestingSupport;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{

    [Collection("marten")]
    public class DurableTcpTransportFixture : SendingComplianceFixture, IAsyncLifetime
    {
        public DurableTcpTransportFixture() : base($"tcp://localhost:{ PortFinder.GetAvailablePort()}/incoming".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            OutboundAddress = $"tcp://localhost:{PortFinder.GetAvailablePort()}/incoming/durable".ToUri();

            await SenderIs(opts =>
            {
                var receivingUri = $"tcp://localhost:{PortFinder.GetAvailablePort()}/incoming/durable".ToUri();
                opts.ListenForMessagesFrom(receivingUri);

                opts.Services.AddMarten(o =>
                {
                    o.Connection(Servers.PostgresConnectionString);
                    o.DatabaseSchemaName = "sender";
                }).IntegrateWithJasper();
            });

            await ReceiverIs(opts =>
            {
                opts.ListenForMessagesFrom(OutboundAddress);

                opts.Services.AddMarten(o =>
                {
                    o.Connection(Servers.PostgresConnectionString);
                    o.DatabaseSchemaName = "receiver";
                }).IntegrateWithJasper();
            });
        }

        public Task DisposeAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }
    }

    [Collection("marten")]
    public class DurableTcpTransportCompliance : SendingCompliance<DurableTcpTransportFixture>
    {

    }
}

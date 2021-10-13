using System;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Testing.Transports.Tcp
{
    public class Sender : JasperOptions
    {
        public Sender()
        {
            Endpoints.ListenForMessagesFrom($"tcp://localhost:2291/incoming/durable".ToUri());

            Extensions.UseMarten(x =>
            {
                x.Connection(Servers.PostgresConnectionString);
                x.DatabaseSchemaName = "sender";
            });

        }
    }

    public class Receiver : JasperOptions
    {
        public Receiver()
        {
            Endpoints.ListenForMessagesFrom($"tcp://localhost:2290/incoming/durable".ToUri());

            Extensions.UseMarten(x =>
            {
                x.Connection(Servers.PostgresConnectionString);
                x.DatabaseSchemaName = "receiver";
            });

        }
    }

    public class DurableTcpTransportFixture : SendingComplianceFixture, IAsyncLifetime
    {
        public DurableTcpTransportFixture() : base($"tcp://localhost:2290/incoming".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            await SenderIs<Sender>();
            await ReceiverIs<Receiver>();
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
        public DurableTcpTransportCompliance(DurableTcpTransportFixture fixture) : base(fixture)
        {
        }
    }
}

using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Schema;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer
{
    [Collection("sqlserver")]
    public abstract class SqlServerContext : IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            var loader = new SqlServerEnvelopeStorageAdmin(new SqlServerSettings{ConnectionString = Servers.SqlServerConnectionString});
            await loader.RecreateAll();
            await initialize();
        }

        protected virtual Task initialize()
        {
            return Task.CompletedTask;
        }

        public virtual Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

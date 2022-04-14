using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer
{
    [Collection("sqlserver")]
    public abstract class SqlServerContext : IAsyncLifetime
    {
        protected SqlServerEnvelopePersistence thePersistence;

        public async Task InitializeAsync()
        {
            thePersistence = new SqlServerEnvelopePersistence(new SqlServerSettings{ConnectionString = Servers.SqlServerConnectionString}, new AdvancedSettings(null), new NullLogger<SqlServerEnvelopePersistence>());
            await thePersistence.RebuildStorage();
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

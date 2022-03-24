using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Schema;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Persistence.Testing.SqlServer
{
    public class SqlServerEnvelopeStorageAdminTests : SqlServerContext
    {

        [Fact]
        public async Task smoke_test_clear_all()
        {
            await thePersistence.ClearAllPersistedEnvelopes();
        }

    }
}

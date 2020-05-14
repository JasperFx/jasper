using InMemoryMediator.Items;
using Jasper;
using Jasper.Persistence.EntityFrameworkCore;
using Jasper.Persistence.EntityFrameworkCore.Codegen;
using Jasper.Persistence.Sagas;
using Jasper.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EFPlusSqlServerConsole
{
    public class JasperConfig : JasperOptions
    {
        public JasperConfig()
        {
            Advanced.StorageProvisioning = StorageProvisioning.Rebuild;

            // Just the normal work to get the connection string out of
            // application configuration
            var connectionString = "Server=localhost;User Id=sa;Password=P@55w0rd;Timeout=5";

            // Setting up Sql Server-backed message persistence
            // This requires a reference to Jasper.Persistence.SqlServer
            Extensions.PersistMessagesWithSqlServer(connectionString);

            // Set up Entity Framework Core as the support
            // for Jasper's transactional middleware
            Extensions.UseEntityFrameworkCorePersistence();

            // Register the EF Core DbContext
            Services.AddDbContext<ItemsDbContext>(
                x => x.UseSqlServer(connectionString),

                // This is important! Using Singleton scoping
                // of the options allows Jasper + Lamar to significantly
                // optimize the runtime pipeline of the handlers that
                // use this DbContext type
                optionsLifetime:ServiceLifetime.Singleton);
        }

    }

}

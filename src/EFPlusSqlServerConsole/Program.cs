using System.Threading.Tasks;
using InMemoryMediator.Items;
using Jasper;
using Jasper.Persistence.EntityFrameworkCore;
using Jasper.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EFPlusSqlServerConsole
{
    internal class Program
    {
        public static Task<int> Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseJasper(opts =>
                {
                    opts.Advanced.StorageProvisioning = StorageProvisioning.Rebuild;

                    // Just the normal work to get the connection string out of
                    // application configuration
                    var connectionString = "Server=localhost;User Id=sa;Password=P@55w0rd;Timeout=5";

                    // Setting up Sql Server-backed message persistence
                    // This requires a reference to Jasper.Persistence.SqlServer
                    opts.Extensions.PersistMessagesWithSqlServer(connectionString);

                    // Set up Entity Framework Core as the support
                    // for Jasper's transactional middleware
                    opts.Extensions.UseEntityFrameworkCorePersistence();

                    // Register the EF Core DbContext
                    opts.Services.AddDbContext<ItemsDbContext>(
                        x => x.UseSqlServer(connectionString),

                        // This is important! Using Singleton scoping
                        // of the options allows Jasper + Lamar to significantly
                        // optimize the runtime pipeline of the handlers that
                        // use this DbContext type
                        optionsLifetime: ServiceLifetime.Singleton);
                })
                .RunJasper(args);
        }
    }
}

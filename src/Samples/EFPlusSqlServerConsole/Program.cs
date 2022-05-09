using System.Threading.Tasks;
using EFPlusSqlServerConsole.Items;
using Jasper;
using Jasper.Persistence.EntityFrameworkCore;
using Jasper.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton.Resources;

namespace EFPlusSqlServerConsole
{
    internal class Program
    {
        public static Task<int> Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseJasper(opts =>
                {
                    // Just the normal work to get the connection string out of
                    // application configuration
                    var connectionString = "Server=localhost,1435;User Id=sa;Password=P@55w0rd;Timeout=5;Encrypt=false";

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
                // This is rebuilding the persistent storage database schema on startup
                // and also clearing any persisted envelope state
                .UseResourceSetupOnStartup(StartupAction.ResetState)
                .RunJasperAsync(args);
        }
    }
}

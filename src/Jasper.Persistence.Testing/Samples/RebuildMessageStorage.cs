using System;
using Jasper.Persistence.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Jasper.Persistence.Testing.Samples
{
    // SAMPLE: SqlServerPersistedMessageApp
    public class MyJasperApp : JasperOptions
    {
        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // Enables Sql Server-backed message persistence using
            // a connection string from your application's appsettings.json
            Extensions.PersistMessagesWithSqlServer(config.GetConnectionString("sqlserver"));


            // If running in development mode, just rebuild the
            // message database storage objects on application startup
            if (hosting.IsDevelopment())
            {
                Advanced.StorageProvisioning = StorageProvisioning.Rebuild;
            }
        }
    }
    // ENDSAMPLE

    // SAMPLE: MyJasperAppFixture
    public class MyJasperAppFixture : IDisposable
    {
        public MyJasperAppFixture()
        {
            Host = JasperHost.For<MyJasperApp>();

            // This extension method will blow away any existing
            // schema items for message persistence in your configured
            // database and then rebuilds the message persistence objects
            // before the *first* integration test runs
            Host.RebuildMessageStorage();
        }

        public IHost Host { get;  }

        public void Dispose()
        {
            Host?.Dispose();
        }


    }

    // An xUnit test fixture that uses our MyJasperAppFixture
    public class IntegrationTester : IClassFixture<MyJasperAppFixture>
    {
        private readonly MyJasperAppFixture _fixture;

        public IntegrationTester(MyJasperAppFixture fixture)
        {
            _fixture = fixture;
        }
    }
    // ENDSAMPLE
}

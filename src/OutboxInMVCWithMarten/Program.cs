using Jasper;
using Jasper.Http;
using Jasper.Marten;
using Marten;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using TestMessages;

namespace OutboxInMVCWithMarten
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseJasper<OutboxSampleApp>()
                .Build();
        }
    }

    public class OutboxSampleApp : JasperRegistry
    {
        public OutboxSampleApp()
        {
            Include<MartenBackedPersistence>();

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection("Host=localhost;Port=5432;Database=postgres;Username=postgres;password=postgres");
            });

            Publish.Message<UserCreated>();
            Publish.Message<UserDeleted>();
            Publish.AllMessagesTo("tcp://localhost:22222/durable");
        }
    }
}

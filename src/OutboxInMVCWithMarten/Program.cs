using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jasper;
using Jasper.Http;
using Jasper.Marten;
using Marten;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestMessages;

namespace OutboxInMVCWithMarten
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();

//            using (var runtime = JasperRuntime.For<OutboxSampleApp>())
//            {
//                Console.ReadKey();
//            }

        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseJasper<OutboxSampleApp>()
                .Build();
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

//            Http.UseStartup<Startup>();

            Publish.Message<UserCreated>();
            Publish.Message<UserDeleted>();
            Publish.AllMessagesTo("tcp://localhost:22222/durable");
        }
    }
}

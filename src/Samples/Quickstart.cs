using System;
using Bootstrapping.Configuration2;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Jasper.Testing.Samples
{
    // SAMPLE: MyAppRegistry
    public class MyAppRegistry : JasperRegistry
    {
    }
    // ENDSAMPLE


    // SAMPLE: ServiceBusApp
    public class ServiceBusApp : JasperRegistry
    {
    }
    // ENDSAMPLE

    public static class Program
    {
        public static void EntryPoint()
        {
            // SAMPLE: QuickStart-Add-To-AspNetCore
            var host = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()

                // Adds Jasper to your ASP.Net Core application
                // with default configuration
                .UseJasper()
                .Start();

            // ENDSAMPLE
        }
    }

    // SAMPLE: QuickStart-InvoiceCreated
    public class InvoiceCreated
    {
        public Guid InvoiceId { get; set; }
    }

    public class InvoiceHandler
    {
        public void Handle(InvoiceCreated created)
        {
            // do something here with the created variable...
        }
    }

    // ENDSAMPLE
}

namespace Jasper.Testing.Samples2
{
    // SAMPLE: MyAppRegistry2
    public class MyAppRegistry : JasperRegistry
    {
        public MyAppRegistry()
        {
            Hosting(x => x.UseKestrel());
        }
    }

    // ENDSAMPLE
}

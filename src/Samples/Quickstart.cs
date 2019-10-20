using System;
using System.Threading.Tasks;
using Bootstrapping.Configuration2;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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
        public static async Task EntryPoint()
        {
            // SAMPLE: QuickStart-Add-To-AspNetCore
            var host = await Host.CreateDefaultBuilder()
                // Adds Jasper to your .Net Core application
                // with its default configuration
                .UseJasper()
                .StartAsync();

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

    }

    // ENDSAMPLE
}

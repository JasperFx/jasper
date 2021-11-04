using System;
using System.Threading.Tasks;
using Jasper;
using Microsoft.Extensions.Hosting;

namespace Samples
{
    // SAMPLE: MyAppOptions
    public class MyAppOptions : JasperOptions
    {
    }
// ENDSAMPLE


// SAMPLE: ServiceBusApp
    public class ServiceBusApp : JasperOptions
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

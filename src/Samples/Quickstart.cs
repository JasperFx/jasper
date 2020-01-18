using System;
using System.Threading.Tasks;

#if NETSTANDARD2_0
using Host = Microsoft.AspNetCore.WebHost;
#else
using Host = Microsoft.Extensions.Hosting.Host;
#endif

namespace Jasper.Testing.Samples
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

namespace Jasper.Testing.Samples2
{
    // SAMPLE: MyAppRegistry2
    public class MyAppOptions : JasperOptions
    {

    }

    // ENDSAMPLE
}

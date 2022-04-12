using Jasper;
using Microsoft.Extensions.Hosting;

namespace DocumentationSamples
{

    public static class Program
    {
        public static async Task EntryPoint()
        {
            #region sample_QuickStart_Add_To_AspNetCore
            var host = await Host.CreateDefaultBuilder()
                // Adds Jasper to your .Net Core application
                // with its default configuration
                .UseJasper()
                .StartAsync();

            #endregion
        }


    }

#region sample_QuickStart_InvoiceCreated
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

#endregion

}

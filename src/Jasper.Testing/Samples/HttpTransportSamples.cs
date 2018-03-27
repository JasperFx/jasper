using Baseline.Dates;
using Jasper.Http;
using Microsoft.AspNetCore.Hosting;

namespace Jasper.Testing.Samples
{
    // SAMPLE: AppUsingHttpTransport
    public class AppUsingHttpTransport : JasperRegistry
    {
        public AppUsingHttpTransport()
        {
            // While *sending* by the HTTP transport is enabled by default,
            // you have to explicitly enable the HTTP transport listening
            Transports.Http.EnableListening(true)

                // The default is 10 seconds
                .ConnectionTimeout(2.Seconds())

                // Override the releative Url of the message
                // listening routes. The default is _messages
                .RelativeUrl("_jasper");

            // You'll have to have Kestrel or some other
            // web server for this to function
            Hosting
                .UseUrls("http://*:5000")
                .UseKestrel();




        }
    }
    // ENDSAMPLE
}

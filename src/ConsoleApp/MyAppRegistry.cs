using Jasper;
using Microsoft.AspNetCore.Hosting;

namespace MyApp
{

    // SAMPLE: MyAppRegistryWithOptions
    public class MyAppRegistry : JasperRegistry
    {
        public MyAppRegistry()
        {
            Http.UseKestrel().UseUrls("http://localhost:3001");

            // TODO -- use the new syntax from GH-163 when it exists
            Channels.ListenForMessagesFrom("jasper://localhost:2222/incoming");
        }
    }
    // ENDSAMPLE
}

using Jasper.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Jasper.Testing.Samples
{
    public class AspNetCoreIntegration
    {
    }

    // SAMPLE: AppWithMiddleware
    public class AppWithMiddleware : JasperRegistry
    {
        public AppWithMiddleware()
        {
            Hosting(x => x.Configure(app =>
            {
                app.UseMiddleware<CustomMiddleware>();

                app.UseJasper();

                // Just to show how you can configure ASP.Net Core
                // middleware that runs after Jasper's RequestDelegate,
                // but do note that Jasper has its own default "not found"
                // behavior
                app.Run(c =>
                {
                    c.Response.StatusCode = 404;

                    return c.Response.WriteAsync("Not found");
                });
            }));
        }
    }
    // ENDSAMPLE


    public class CustomMiddleware
    {
    }
}

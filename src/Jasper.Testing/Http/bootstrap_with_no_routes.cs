using System.Threading.Tasks;
using Alba;
using Jasper.TestSupport.Alba;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Jasper.Testing.Http
{
    public class bootstrap_with_no_routes
    {
        [Fact]
        public async Task will_still_apply_middleware()
        {
            var runtime = await JasperRuntime.ForAsync<JasperRegistry>(_ =>
            {
                _.HttpRoutes.DisableConventionalDiscovery();
                _.Hosting.Configure(app => { app.Run(c => c.Response.WriteAsync("Hello")); });
            });

            try
            {
                await runtime.Scenario(s =>
                {
                    s.Get.Url("/");
                    s.ContentShouldBe("Hello");
                });
            }
            finally
            {
                await runtime.Shutdown();
            }
        }
    }
}

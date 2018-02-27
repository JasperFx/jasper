using System.Threading.Tasks;
using Alba;
using JasperHttpTesting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Jasper.Http.Testing
{
    public class bootstrap_with_no_routes
    {
        [Fact]
        public async Task will_still_apply_middleware()
        {
            using (var runtime = JasperRuntime.For<JasperHttpRegistry>(_ =>
            {
                _.Http.Actions.DisableConventionalDiscovery();
                _.Http.Configure(app =>
                {
                    app.Run(c => c.Response.WriteAsync("Hello"));
                });
            }))
            {
                await runtime.Scenario(s =>
                {
                    s.Get.Url("/");
                    s.ContentShouldBe("Hello");
                });
            }
        }
    }
}

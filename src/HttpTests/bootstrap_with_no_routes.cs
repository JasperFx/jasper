using System.Threading.Tasks;
using Alba;
using Jasper;
using Jasper.TestSupport.Alba;
using JasperHttp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace HttpTests
{
    public class bootstrap_with_no_routes
    {
        [Fact]
        public async Task will_still_apply_middleware()
        {
            var runtime = JasperHost.CreateDefaultBuilder()
                .Configure(app =>
                {
                    app.Run(c => c.Response.WriteAsync("Hello"));
                })
                .UseJasper(_ => _.Http(opts => opts.DisableConventionalDiscovery()))
                .ToAlbaSystem();


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
                runtime.Dispose();
            }
        }
    }
}

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.AspNetCoreIntegration
{
    public class can_bootstrap_a_bus_plus_aspnetcore_app_through_jasper_registry
    {

        [Fact]
        public async Task can_handle_an_http_request_through_Kestrel()
        {
            var theRuntime = await JasperRuntime.ForAsync<JasperServerApp>();

            try
            {
                using (var client = new HttpClient())
                {
                    var text = await client.GetStringAsync("http://localhost:5200");
                    text.ShouldContain("Hello from a hybrid Jasper application");
                }

                // has the message context registered
                theRuntime.Get<IMessageContext>().ShouldNotBeNull();

                // has the registrations from Jasper
                theRuntime.Get<IFoo>().ShouldBeOfType<Foo>();
            }
            finally
            {
                await theRuntime.Shutdown();
            }
        }

        [Fact]
        public async Task can_delegate_to_mvc_route_through_Kestrel()
        {
            var theRuntime = await JasperRuntime.ForAsync<JasperServerApp>();


            try
            {
                using (var client = new HttpClient())
                {
                    var text = await client.GetStringAsync("http://localhost:5200/values/5");
                    text.ShouldContain("5");
                }
            }
            finally
            {
                await theRuntime.Shutdown();
            }
        }

    }

    public class SomeHandler
    {
        public void Handle(SomeMessage message)
        {

        }
    }

    public class SomeMessage
    {

    }

    // SAMPLE: ConfiguringAspNetCoreWithinJasperRegistry
    public class JasperServerApp : JasperRegistry
    {
        public JasperServerApp()
        {
            Handlers.Discovery(x => x.DisableConventionalDiscovery());

            Hosting
                .UseKestrel()
                .UseUrls("http://localhost:5200")
                .UseStartup<Startup>();

        }
    }
    // ENDSAMPLE

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseJasper();
            app.UseMvc();
            app.Run(c => c.Response.WriteAsync("Hello from a hybrid Jasper application"));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddTransient<IFoo, Foo>();
        }
    }

    public interface IFoo{}
    public class Foo : IFoo{}
}

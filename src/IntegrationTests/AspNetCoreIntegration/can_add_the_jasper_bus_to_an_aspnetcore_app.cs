using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper;
using Jasper.Bus;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Testing;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Http;
using Jasper.Testing.Samples;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace IntegrationTests.AspNetCoreIntegration
{
    public class can_add_the_jasper_bus_to_an_aspnetcore_app : IDisposable
    {
        private IWebHost theHost;

        public can_add_the_jasper_bus_to_an_aspnetcore_app()
        {
            // SAMPLE: adding-jasper-to-aspnetcore-app
            var builder = new WebHostBuilder();
            builder
                .UseKestrel()
                .UseUrls("http://localhost:3003")
                .UseStartup<Startup>()
                .UseJasper<SimpleJasperBusApp>();


            theHost = builder.Build();

            theHost.Start();
            // ENDSAMPLE

        }

        public void sample()
        {
            // SAMPLE: ordering-middleware-with-jasper
            var builder = new WebHostBuilder();
            builder
                .UseKestrel()
                .UseUrls("http://localhost:3003")
                .Configure(app =>
                {
                    app.UseMiddleware<CustomMiddleware>();

                    // Add the Jasper middleware
                    app.AddJasper();

                    // Nothing stopping you from using Jasper *and*
                    // MVC, NancyFx, or any other ASP.Net Core
                    // compatible framework
                    app.AddMvc();
                })
                .UseJasper<SimpleJasperBusApp>();


            theHost = builder.Build();

            theHost.Start();
            // ENDSAMPLE
        }

        public void Dispose()
        {
            theHost?.Dispose();
        }

        [Fact]
        public async Task can_handle_an_http_request_through_Kestrel()
        {
            using (var client = new HttpClient())
            {
                var text = await client.GetStringAsync("http://localhost:3003");
                SpecificationExtensions.ShouldContain("Hello from a hybrid Jasper application", text);

            }
        }

        [Fact]
        public void has_the_bus()
        {
            theHost.Services.GetService(typeof(IServiceBus))
                .ShouldNotBeNull();
        }

        [Fact]
        public void captures_registrations_from_configure_registry()
        {
            theHost.Services.GetService(typeof(IFoo)).ShouldBeOfType<Foo>();
        }

        [Fact]
        public void is_definitely_using_a_StructureMap_service_provider()
        {
            theHost.Services.ShouldBeOfType<StructureMapServiceProvider>();
        }
    }

    // SAMPLE: SimpleJasperBusApp
    public class SimpleJasperBusApp : JasperRegistry
    // ENDSAMPLE
    {
        public SimpleJasperBusApp()
        {
            Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();
            Services.AddTransient<IFakeService, FakeService>();
            Services.AddTransient<IWidget, Widget>();
        }
    }

    public static class FakeExtensions
    {
        public static IApplicationBuilder AddMvc(this IApplicationBuilder app)
        {
            return app;
        }
    }
}


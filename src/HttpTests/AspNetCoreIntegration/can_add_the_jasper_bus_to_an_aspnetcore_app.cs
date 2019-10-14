using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper;
using Jasper.Messaging;
using JasperHttp;
using Lamar;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace HttpTests.AspNetCoreIntegration
{
    public class AspNetCombinedFixture : IDisposable
    {
        public IHost theHost;

        public AspNetCombinedFixture()
        {
            // SAMPLE: adding-jasper-to-aspnetcore-app
            var builder = Host.CreateDefaultBuilder();
            builder
                .ConfigureWebHostDefaults(x =>
                {
                    x.UseUrls("http://localhost:3003")
                    .UseStartup<Startup>();
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
    }

    public class Samples
    {
        public void bootstrap()
        {
            // SAMPLE: simplest-aspnetcore-bootstrapping
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(x => x.UseStartup<Startup>())
                .UseJasper() // Use Jasper with all its
                // defaults
                .Start();
            // ENDSAMPLE
        }

        /*
        // SAMPLE: simplest-aspnetcore-run-from-command-line
        public static int Main(params string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .UseJasper();

            return JasperAgent.Run(builder, args);
        }
        // ENDSAMPLE
        */


        /*
        // SAMPLE: simplest-aspnetcore-run-from-command-line-2
        public static int Main(params string[] args)
        {
            return WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .UseJasper()
                .RunJasper(args);
        }
        // ENDSAMPLE
        */


        /*
        // SAMPLE: simplest-idiomatic-command-line
        public static int Main(params string[] args)
        {
            return JasperAgent.RunBasic(args);
        }
        // ENDSAMPLE
        */
    }

    public class can_add_jasper_to_default_web_host_builder
    {
        [Fact]
        public async Task still_works()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(x =>
                {
                    x.UseUrls("http://localhost:5024")
                    .UseKestrel(x => x.ListenLocalhost(5024))
                    .UseStartup<EmptyStartup>();
                })

                .UseJasper<SimpleJasperBusApp>();

            using (var theHost = builder.Build())
            {
                await theHost.StartAsync();

                using (var client = new HttpClient())
                {
                    var text = await client.GetStringAsync("http://localhost:5024/hello");

                    // See "get_hello" method
                    text.ShouldContain("hello");
                }
            }
        }
    }

    public class can_add_the_jasper_bus_to_an_aspnetcore_app : IClassFixture<AspNetCombinedFixture>
    {
        public can_add_the_jasper_bus_to_an_aspnetcore_app(AspNetCombinedFixture fixture)
        {
            theHost = fixture.theHost;
        }

        private readonly IHost theHost;

        // ReSharper disable once UnusedMember.Global
        protected void sample()
        {
            // SAMPLE: ordering-middleware-with-jasper
            var builder = Host.CreateDefaultBuilder();
            builder
                .ConfigureWebHostDefaults(web =>
                {
                    web.UseKestrel(x => x.ListenLocalhost(3003))
                    .UseUrls("http://localhost:3003")
                    .ConfigureServices(s => s.AddMvc())
                    .Configure(app =>
                    {
                        app.UseMiddleware<CustomMiddleware>();

                        // Add the Jasper middleware
                        app.UseJasper();

                        // Nothing stopping you from using Jasper *and*
                        // MVC, NancyFx, or any other ASP.Net Core
                        // compatible framework
                        app.UseMvc();
                    });
                })

                .UseJasper<SimpleJasperBusApp>();


            var host = builder.Build();

            host.Start();
            // ENDSAMPLE
        }


        [Fact]
        public async Task can_handle_an_http_request_through_Kestrel_to_MVC_route()
        {
            using (var client = new HttpClient())
            {
                var text = await client.GetStringAsync("http://localhost:3003/values");
                text.ShouldContain("Hello from MVC Core");
            }
        }

        [Fact]
        public void captures_registrations_from_configure_registry()
        {
            theHost.Services.GetService(typeof(IFoo)).ShouldBeOfType<Foo>();
        }

        [Fact]
        public void has_the_bus()
        {
            theHost.Services.GetService(typeof(IMessageContext)).ShouldNotBeNull();
        }

        [Fact]
        public void is_definitely_using_a_StructureMap_service_provider()
        {
            theHost.Services.ShouldBeOfType<Container>();
        }
    }

    // SAMPLE: SimpleJasperBusApp
    public class SimpleJasperBusApp : JasperRegistry
        // ENDSAMPLE
    {
    }


    public class CustomMiddleware
    {
    }
}

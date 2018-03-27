using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Jasper.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing
{
    // TODO -- use a Collection<T> on this one. Or go to ST
    public class bootstrapping_end_to_end : IDisposable
    {
        public bootstrapping_end_to_end()
        {
            registry = new JasperRegistry();
            registry.Http.Actions.ExcludeTypes(_ => _.IsInNamespace("Jasper.Bus"));

            registry.Include<EndpointExtension>();

            registry.Handlers.DisableConventionalDiscovery();

            registry.Services.AddTransient<IWriterRule, CustomWriterRule>();
            registry.Services.AddTransient<IReaderRule, CustomReaderRule>();


        }

        private async Task withApp()
        {
            theRuntime = await JasperRuntime.ForAsync(registry);
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        private JasperRuntime theRuntime;
        private JasperRegistry registry;


        [Fact]
        public async Task can_apply_custom_conneg_rules()
        {
            await withApp();

            var rules = theRuntime.Container.QuickBuild<ConnegRules>();
            rules.Readers.First().ShouldBeOfType<CustomReaderRule>();
            rules.Writers.First().ShouldBeOfType<CustomWriterRule>();
        }

        [Fact]
        public async Task can_discover_endpoints_from_static_types()
        {
            await withApp();

            theRuntime.Get<RouteGraph>().Gets.Any(x => x.Route.HandlerType == typeof(StaticEndpoint))
                .ShouldBeTrue();
        }

        [Fact]
        public async Task can_find_and_apply_endpoints_by_type_scanning()
        {
            await withApp();

            var routes = theRuntime.Get<RouteGraph>();

            routes.Any(x => x.Action.HandlerType == typeof(SimpleEndpoint)).ShouldBeTrue();
            routes.Any(x => x.Action.HandlerType == typeof(OtherEndpoint)).ShouldBeTrue();
        }

        [Fact]
        public async Task can_import_endpoints_from_extension_includes()
        {
            await withApp();

            theRuntime.Get<RouteGraph>().Gets.Any(x => x.Route.HandlerType == typeof(ExtensionThing))
                .ShouldBeTrue();
        }

        [Fact]
        public async Task can_use_custom_hosted_service_without_aspnet()
        {
            await withApp();

            var service = new CustomHostedService();

            var runtime = JasperRuntime.For<JasperRegistry>(_ => _.Services.AddSingleton<IHostedService>(service));

            service.WasStarted.ShouldBeTrue();
            service.WasStopped.ShouldBeFalse();

            runtime.Dispose();

            service.WasStopped.ShouldBeTrue();
        }

        [Fact]
        public async Task reverse_url_lookup_by_input_model()
        {
            await withApp();

            var urls = theRuntime.Get<IUrlRegistry>();
            urls.UrlFor<OtherModel>().ShouldBe("/other");
        }

        [Fact]
        public async Task reverse_url_lookup_by_method()
        {
            await withApp();

            var urls = theRuntime.Get<IUrlRegistry>();
            urls.UrlFor<SimpleEndpoint>(x => x.get_hello()).ShouldBe("/hello");
        }

        [Fact]
        public async Task route_graph_is_registered()
        {
            await withApp();

            theRuntime.Get<RouteGraph>()
                .Any().ShouldBeTrue();
        }

        [Fact]
        public async Task url_registry_is_registered()
        {
            await withApp();

            theRuntime.Get<IUrlRegistry>().ShouldNotBeNull();
        }
    }

    public class CustomHostedService : IHostedService
    {
        public bool WasStarted { get; set; }

        public bool WasStopped { get; set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            WasStarted = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WasStopped = true;
            return Task.CompletedTask;
        }
    }

    public class EndpointExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Http.Actions.IncludeType<ExtensionThing>();
        }
    }

    public class ExtensionThing
    {
        public string get_hello_from_extension()
        {
            return "hello";
        }
    }


    public class CustomReaderRule : IReaderRule
    {
        public bool TryToApply(RouteChain chain)
        {
            return false;
        }
    }

    public class CustomWriterRule : IWriterRule
    {
        public bool TryToApply(RouteChain chain)
        {
            return false;
        }
    }

    public class StaticEndpoint
    {
        public static string get_static_message()
        {
            return "hey";
        }
    }

    public class SimpleEndpoint
    {
        public string get_hello()
        {
            return "hello";
        }

        public static Task put_simple(HttpResponse response)
        {
            response.ContentType = "text/plain";
            return response.WriteAsync("Simple is as simple does");
        }
    }

    public class OtherEndpoint
    {
        public void delete_other(HttpContext context)
        {
            context.Items.Add("delete_other", "called");
        }

        public void put_other(OtherModel input)
        {
        }
    }

    public class OtherModel
    {
    }
}

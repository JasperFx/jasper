using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BlueMilk.Scanning;
using Jasper.Configuration;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http
{
    public class bootstrapping_end_to_end : IDisposable
    {
        public bootstrapping_end_to_end()
        {
            TypeRepository.ClearAll();

            var registry = new JasperRegistry();
            registry.Http.Actions.ExcludeTypes(_ => _.IsInNamespace("Jasper.Bus"));

            registry.Include<EndpointExtension>();

            registry.Handlers.DisableConventionalDiscovery();

            registry.Services.AddTransient<IWriterRule, CustomWriterRule>();
            registry.Services.AddTransient<IReaderRule, CustomReaderRule>();

            theRuntime = JasperRuntime.For(registry);
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        private readonly JasperRuntime theRuntime;

        [Fact]
        public void can_apply_custom_conneg_rules()
        {
            var rules = theRuntime.Get<ConnegRules>();
            rules.Readers.First().ShouldBeOfType<CustomReaderRule>();
            rules.Writers.First().ShouldBeOfType<CustomWriterRule>();
        }

        [Fact]
        public void can_discover_endpoints_from_static_types()
        {
            theRuntime.Get<RouteGraph>().Gets.Any(x => x.Route.HandlerType == typeof(StaticEndpoint))
                .ShouldBeTrue();
        }

        [Fact]
        public void can_find_and_apply_endpoints_by_type_scanning()
        {
            var routes = theRuntime.Get<RouteGraph>();

            routes.Any(x => x.Action.HandlerType == typeof(SimpleEndpoint)).ShouldBeTrue();
            routes.Any(x => x.Action.HandlerType == typeof(OtherEndpoint)).ShouldBeTrue();
        }

        [Fact]
        public void can_import_endpoints_from_extension_includes()
        {
            theRuntime.Get<RouteGraph>().Gets.Any(x => x.Route.HandlerType == typeof(ExtensionThing))
                .ShouldBeTrue();
        }

        [Fact]
        public void reverse_url_lookup_by_input_model()
        {
            var urls = theRuntime.Get<IUrlRegistry>();
            urls.UrlFor<OtherModel>().ShouldBe("/other");
        }

        [Fact]
        public void reverse_url_lookup_by_method()
        {
            var urls = theRuntime.Get<IUrlRegistry>();
            urls.UrlFor<SimpleEndpoint>(x => x.get_hello()).ShouldBe("/hello");
        }

        [Fact]
        public void route_graph_is_registered()
        {
            theRuntime.Get<RouteGraph>()
                .Any().ShouldBeTrue();
        }

        [Fact]
        public void url_registry_is_registered()
        {
            theRuntime.Get<IUrlRegistry>().ShouldNotBeNull();
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

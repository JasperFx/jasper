using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Http;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Jasper.Testing.Bus.Compilation;
using Microsoft.AspNetCore.Http;
using Shouldly;
using StructureMap.Graph.Scanning;
using StructureMap.TypeRules;
using Xunit;

namespace Jasper.Testing.Http
{
    public class bootstrapping_end_to_end : IDisposable
    {
        private readonly JasperRuntime theRuntime;

        public bootstrapping_end_to_end()
        {
            TypeRepository.ClearAll();

            var registry = new JasperRegistry();
            registry.Http.Actions.ExcludeTypes(_ => _.IsInNamespace("Jasper.Bus"));

            registry.Handlers.ExcludeTypes(x => true);

            registry.Services.AddTransient<IWriterRule, CustomWriterRule>();
            registry.Services.AddTransient<IReaderRule, CustomReaderRule>();

            theRuntime = JasperRuntime.For(registry);
        }

        [Fact]
        public void route_graph_is_registered()
        {
            theRuntime.Get<RouteGraph>()
                .Any().ShouldBeTrue();
        }

        [Fact]
        public void can_find_and_apply_endpoints_by_type_scanning()
        {
            var routes = theRuntime.Get<RouteGraph>();

            routes.Any(x => x.Action.HandlerType == typeof(SimpleEndpoint)).ShouldBeTrue();
            routes.Any(x => x.Action.HandlerType == typeof(OtherEndpoint)).ShouldBeTrue();
        }

        [Fact]
        public void url_registry_is_registered()
        {
            theRuntime.Container.Model.HasDefaultImplementationFor<IUrlRegistry>().ShouldBeTrue();
        }

        [Fact]
        public void reverse_url_lookup_by_method()
        {
            var urls = theRuntime.Get<IUrlRegistry>();
            urls.UrlFor<SimpleEndpoint>(x => x.get_hello()).ShouldBe("/hello");
        }

        [Fact]
        public void reverse_url_lookup_by_input_model()
        {
            var urls = theRuntime.Get<IUrlRegistry>();
            urls.UrlFor<OtherModel>().ShouldBe("/other");
        }

        [Fact]
        public void can_apply_custom_conneg_rules()
        {
            var rules = theRuntime.Get<ConnegRules>();
            rules.Readers.First().ShouldBeOfType<CustomReaderRule>();
            rules.Writers.First().ShouldBeOfType<CustomWriterRule>();
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
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

    public class OtherModel { }
}

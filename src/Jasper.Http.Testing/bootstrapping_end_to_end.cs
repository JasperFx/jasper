﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Lamar;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Http.Testing
{
    public class HttpBootstrappedApp : JasperOptions
    {
        public HttpBootstrappedApp()
        {
            Extensions.Include<EndpointExtension>();

            Handlers.DisableConventionalDiscovery();

            Services.AddTransient<IWriterRule, CustomWriterRule>();
            Services.AddTransient<IReaderRule, CustomReaderRule>();

            Extensions.ConfigureHttp(x =>
            {
                x.DisableConventionalDiscovery();
                x.IncludeType<SimpleEndpoint>();
                x.IncludeType<StaticEndpoint>();
                x.IncludeType<OtherEndpoint>();
            });
        }
    }



    public class bootstrapping_end_to_end : RegistryContext<HttpBootstrappedApp>
    {
        public bootstrapping_end_to_end(RegistryFixture<HttpBootstrappedApp> fixture) : base(fixture)
        {
            // Route finding is lazy apparently
            scenario(x => x.Get.Url("/hello")).GetAwaiter().GetResult();
        }

        [Fact]
        public void can_apply_custom_conneg_rules()
        {
            var rules = System.Services.As<Container>().QuickBuild<ConnegRules>();
            rules.Readers.First().ShouldBeOfType<CustomReaderRule>();
            rules.Writers.ElementAt(1).ShouldBeOfType<CustomWriterRule>();
        }

        [Fact]
        public void can_discover_endpoints_from_static_types()
        {
            var routes = System.Services.GetRequiredService<RouteGraph>();
            routes.Gets.Any(x => x.Route.HandlerType == typeof(StaticEndpoint))
                .ShouldBeTrue();
        }

        [Fact]
        public void can_find_and_apply_endpoints_by_type_scanning()
        {
            var routes = System.Services.GetRequiredService<RouteGraph>();

            routes.Any(x => x.Action.HandlerType == typeof(SimpleEndpoint)).ShouldBeTrue();
            routes.Any(x => x.Action.HandlerType == typeof(OtherEndpoint)).ShouldBeTrue();
        }

        [Fact]
        public void can_import_endpoints_from_extension_includes()
        {
            System.Services.GetRequiredService<RouteGraph>().Gets
                .Any(x => x.Route.HandlerType == typeof(ExtensionThing))
                .ShouldBeTrue();
        }

        [Fact]
        public void reverse_url_lookup_by_input_model()
        {
            var urls = System.Services.GetRequiredService<IUrlRegistry>();
            urls.UrlForType<OtherModel>().ShouldBe("/other");
        }

        [Fact]
        public void reverse_url_lookup_by_method()
        {
            var urls = System.Services.GetRequiredService<IUrlRegistry>();
            urls.UrlFor<SimpleEndpoint>(x => x.get_hello()).ShouldBe("/hello");
        }

        [Fact]
        public void route_graph_is_registered()
        {
            System.Services.GetRequiredService<RouteGraph>()
                .Any().ShouldBeTrue();
        }

        [Fact]
        public void url_registry_is_registered()
        {
            System.Services.GetRequiredService<IUrlRegistry>().ShouldNotBeNull();
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
        public void Configure(JasperOptions registry)
        {
            registry.Extensions.ConfigureHttp(x => x.IncludeType<ExtensionThing>());
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

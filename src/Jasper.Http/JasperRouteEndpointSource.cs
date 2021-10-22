using System;
using System.Collections.Generic;
using System.Linq;
using Lamar;
using LamarCodeGeneration;
using LamarCompiler;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Jasper.Http
{
    public class JasperRouteEndpointSource : EndpointDataSource
    {
        private readonly IContainer _container;
        private readonly Action<JasperHttpOptions> _customization;
        private Endpoint[] _endpoints;

        public JasperRouteEndpointSource(IContainer container, Action<JasperHttpOptions> customization)
        {
            _container = container;
            _customization = customization;
        }

        public override IReadOnlyList<Endpoint> Endpoints => _endpoints ?? (_endpoints = BuildEndpoints().ToArray());

        public override IChangeToken GetChangeToken()
        {
            return NullChangeToken.Singleton;
        }

        private IEnumerable<Endpoint> BuildEndpoints()
        {
            var builder = _container.QuickBuild<RouteBuilder>();
            return builder.BuildEndpoints(_customization);
        }

        internal class RouteBuilder
        {
            private readonly IContainer _container;
            private readonly JasperHttpOptions _httpOptions;
            private readonly JasperOptions _options;

            public RouteBuilder(JasperHttpOptions httpOptions, IContainer container, JasperOptions options)
            {
                _httpOptions = httpOptions;
                _container = container;
                _options = options;
            }

            public IEnumerable<Endpoint> BuildEndpoints(Action<JasperHttpOptions> customization)
            {
                var graph = _httpOptions.Routes;
                graph.Container = _container;

                // One time customization
                customization?.Invoke(_httpOptions);

                var actions = _httpOptions.FindActions(_options.ApplicationAssembly).GetAwaiter().GetResult();

                // TODO -- Need to apply policies!

                foreach (var action in actions)
                {
                    var chain = graph.AddRoute(action);
                    _httpOptions.Urls.Register(chain.Route);
                }


                graph.AssertNoDuplicateRoutes();

                var rules = _options.Advanced.CodeGeneration;
                _httpOptions.ApplyPolicies(rules);

                var generatedAssembly = new GeneratedAssembly(rules);

                var services = graph.AssemblyTypes(rules, generatedAssembly);
                new AssemblyGenerator().Compile(generatedAssembly, services);


                foreach (var chain in graph) yield return chain.BuildEndpoint(_container);
            }
        }
    }
}

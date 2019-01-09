using System;
using System.Linq;
using System.Threading.Tasks;
using Alba;
using benchmarks.Routes;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Jasper;
using Jasper.Http.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace benchmarks
{
    [SimpleJob(warmupCount: 2)]
    [MemoryDiagnoser]
    public class RoutingBenchmark : IDisposable
    {
        private TestRoute[] _routes;
        private SystemUnderTest _system;

        public RoutingBenchmark()
        {
            var builder = new WebHostBuilder().UseJasper().Configure(app => app.UseJasper());

            _system = new SystemUnderTest(builder);

            var graph = _system.Services.GetRequiredService<RouteGraph>();

            _routes = graph
                .Select(x => x.Action.HandlerType)
                .Distinct()
                .Select(Activator.CreateInstance)
                .OfType<IHasUrls>()
                .SelectMany(x => x.Urls().Select(url => new TestRoute(x.Method, url)))
                .ToArray();
        }

        [Benchmark]
        public async Task RunRequests()
        {
            foreach (var testRoute in _routes)
            {
                await testRoute.Run(_system);
            }
        }

        public void Dispose()
        {
            _system?.Dispose();
        }
    }

    public class TestRoute
    {
        public string Method { get; }
        public string Url { get; }

        public TestRoute(string method, string url)
        {
            Method = method;
            Url = url;
        }

        public Task Run(SystemUnderTest system)
        {
            switch (Method)
            {
                case "GET":
                    return system.Scenario(x => x.Get.Url(Url));

                case "POST":
                    return system.Scenario(x => x.Post.Url(Url));

                case "PUT":
                    return system.Scenario(x => x.Put.Url(Url));

                case "DELETE":
                    return system.Scenario(x => x.Delete.Url(Url));

                case "HEAd":
                    return system.Scenario(x => x.Head.Url(Url));
            }

            throw new Exception("Bad request!");
        }
    }
}

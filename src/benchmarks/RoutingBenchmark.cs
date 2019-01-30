using System;
using System.Linq;
using System.Threading.Tasks;
using Alba;
using benchmarks.Routes;
using Baseline;
using BenchmarkDotNet.Attributes;
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
                .Where(x => x.IsConcreteWithDefaultCtor())
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
}

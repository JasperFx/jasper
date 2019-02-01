using System;
using System.Linq;
using System.Threading.Tasks;
using Alba;
using benchmarks.Routes;
using Baseline;
using BenchmarkDotNet.Attributes;
using Jasper;
using Jasper.Http;
using Jasper.Http.Model;
using Jasper.MvcExtender;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace benchmarks
{
    [SimpleJob(warmupCount: 2)]
    [MemoryDiagnoser]
    public class FastModeRoutingBenchmark : IDisposable
    {
        private TestRoute[] _routes;
        private SystemUnderTest _system;
        private SystemUnderTest _aspnetSystem;
        private SystemUnderTest _hybridSystem;

        public FastModeRoutingBenchmark()
        {
            var builder = new WebHostBuilder()
                .ConfigureLogging(x => { x.ClearProviders(); })
                .UseJasper(x => x.HttpRoutes.AspNetCoreCompliance = ComplianceMode.GoFaster)
                .Configure(app => app.UseJasper());

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


            var aspnetBuilder = AspNetCorePerfTarget.Program.CreateWebHostBuilder(new string[0])
                .UseStartup<AspNetCorePerfTarget.Startup>();

            _aspnetSystem = new SystemUnderTest(aspnetBuilder);

            var hybridBuilder = JasperHost.CreateDefaultBuilder().UseJasper<HybridRegistry>();

            _hybridSystem = new SystemUnderTest(hybridBuilder);
        }

        public class HybridRegistry : JasperRegistry
        {
            public HybridRegistry() : base("AspNetCorePerfTarget")
            {
                Include<MvcExtenderExtension>();
            }
        }

        [Benchmark]
        public async Task RunRequests()
        {
            foreach (var testRoute in _routes)
            {
                await testRoute.Run(_system);
            }
        }

        [Benchmark]
        public async Task RunAspNetRequests()
        {
            foreach (var testRoute in _routes)
            {
                await testRoute.Run(_aspnetSystem);
            }
        }
/*


        [Benchmark]
        public async Task RunHybridRequests()
        {
            foreach (var testRoute in _routes)
            {
                await testRoute.Run(_hybridSystem);
            }
        }
*/


        public void Dispose()
        {
            _system?.Dispose();
            _aspnetSystem?.Dispose();
            _hybridSystem?.Dispose();
        }

    }

}

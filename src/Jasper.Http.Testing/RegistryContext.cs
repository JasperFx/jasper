using System;
using System.Threading.Tasks;
using Alba;
using Baseline;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestingSupport;
using Xunit;

namespace Jasper.Http.Testing
{
    public class BasicAppNoHandling : JasperOptions
    {
        public BasicAppNoHandling()
        {
            Handlers.DisableConventionalDiscovery();

            Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });
        }
    }

    public class JasperTestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(x => x.MapJasperEndpoints());
        }
    }

    public class RegistryFixture<T> : IDisposable where T : JasperOptions, new()
    {
        public RegistryFixture()
        {
            var builder = Host.CreateDefaultBuilder()
                .UseJasper<T>()
                .ConfigureWebHostDefaults(x => x.UseStartup<JasperTestStartup>());

            System = new SystemUnderTest(builder);
        }

        public SystemUnderTest System { get; }


        public void Dispose()
        {
            System.Services.As<Container>().DisposalLock = DisposalLock.Unlocked;
            System?.Dispose();
        }

        public Task<IScenarioResult> Scenario(Action<Scenario> configuration)
        {
            return System.Scenario(configuration);
        }
    }

    [Collection("integration")]
    public class RegistryContext<T> : IClassFixture<RegistryFixture<T>> where T : JasperOptions, new()
    {
        private readonly RegistryFixture<T> _fixture;

        public RegistryContext(RegistryFixture<T> fixture)
        {
            _fixture = fixture;
        }

        protected SystemUnderTest System => _fixture.System;


        protected Task<IScenarioResult> scenario(Action<Scenario> configuration)
        {
            return _fixture.Scenario(configuration);
        }
    }
}

using System;
using System.Threading.Tasks;
using Alba;
using Baseline;
using Jasper;
using Jasper.Messaging.Model;
using Jasper.TestSupport.Alba;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using TestingSupport;
using Xunit;

namespace HttpTests
{
    public class BasicAppNoHandling : JasperRegistry
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


    public class RegistryFixture<T> : IDisposable where T : JasperRegistry, new()
    {
        private readonly Lazy<SystemUnderTest> _sut = new Lazy<SystemUnderTest>(() =>
        {
            var system = JasperAlba.For<T>();
            system.Services.As<Container>().DisposalLock = DisposalLock.ThrowOnDispose;

            return system;
        });

        public SystemUnderTest System => _sut.Value;

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
    public class RegistryContext<T> : IClassFixture<RegistryFixture<T>> where T : JasperRegistry, new()
    {
        private readonly RegistryFixture<T> _fixture;

        public RegistryContext(RegistryFixture<T> fixture)
        {
            _fixture = fixture;
        }

        protected SystemUnderTest Runtime => _fixture.System;


        protected HandlerGraph theHandlers()
        {
            return _fixture.System.Services.GetRequiredService<HandlerGraph>();
        }

        protected Task<IScenarioResult> scenario(Action<Scenario> configuration)
        {
            return _fixture.Scenario(configuration);
        }
    }
}

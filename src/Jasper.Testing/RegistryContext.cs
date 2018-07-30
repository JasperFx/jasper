using System;
using System.Threading.Tasks;
using Alba;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Stub;
using Jasper.Testing.Messaging.Bootstrapping;
using Jasper.TestSupport.Alba;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jasper.Testing
{
    public class BasicAppNoHandling : JasperRegistry
    {
        public BasicAppNoHandling()
        {
            Handlers.DisableConventionalDiscovery();


            Services.AddSingleton<ITransport, StubTransport>();

            Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });
        }
    }



    public class RegistryFixture<T> : IDisposable where T : JasperRegistry, new()
    {
        private JasperRuntime _runtime;

        public async Task WithApp()
        {
            if (_runtime == null)
            {
                _runtime = await JasperRuntime.ForAsync<T>();
            }
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        public JasperRuntime Runtime => _runtime;

        public Task<IScenarioResult> Scenario(Action<Scenario> configuration)
        {
            if (_runtime != null)
            {
                return _runtime.Scenario(configuration);
            }
            else
            {
                return JasperRuntime.ForAsync<T>()
                    .ContinueWith(t => { _runtime = t.Result; })
                    .ContinueWith(t => _runtime.Scenario(configuration))
                    .Unwrap();
            }
        }
    }

    public class RegistryContext<T> : IClassFixture<RegistryFixture<T>> where T : JasperRegistry, new()
    {
        private readonly RegistryFixture<T> _fixture;

        public RegistryContext(RegistryFixture<T> fixture)
        {
            _fixture = fixture;
        }

        protected JasperRuntime Runtime => _fixture.Runtime;

        protected Task withApp()
        {
            return _fixture.WithApp();
        }

        protected async Task<HandlerGraph> theHandlers()
        {
            await _fixture.WithApp();

            return _fixture.Runtime.Get<HandlerGraph>();
        }

        protected Task<IScenarioResult> scenario(Action<Scenario> configuration)
        {
            return _fixture.Scenario(configuration);
        }
    }
}

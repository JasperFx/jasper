using System;
using System.Threading.Tasks;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Stub;
using Jasper.Testing.Messaging.Bootstrapping;
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

        public Task WithApp()
        {
            if (_runtime == null)
            {
                return JasperRuntime.ForAsync<T>();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        public JasperRuntime Runtime => _runtime;


    }

    public class RegistryContext<T> : IClassFixture<RegistryFixture<T>> where T : JasperRegistry, new()
    {
        private readonly RegistryFixture<T> _fixture;

        public RegistryContext(RegistryFixture<T> fixture)
        {
            _fixture = fixture;
        }

        protected JasperRuntime Runtime => _fixture.Runtime;

        public async Task<HandlerGraph> theHandlers()
        {
            await _fixture.WithApp();

            return _fixture.Runtime.Get<HandlerGraph>();
        }
    }
}

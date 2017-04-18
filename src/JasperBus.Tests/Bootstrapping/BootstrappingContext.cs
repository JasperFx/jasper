using System;
using System.Linq;
using Jasper;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Tests.Stubs;

namespace JasperBus.Tests.Bootstrapping
{
    public class BootstrappingContext : IDisposable
    {
        public readonly Uri Uri1 = new Uri("stub://1");
        public readonly Uri Uri2 = new Uri("stub://2");
        public readonly Uri Uri3 = new Uri("stub://3");
        public readonly Uri Uri4 = new Uri("stub://4");

        public readonly JasperBusRegistry theRegistry = new JasperBusRegistry();

        private Lazy<JasperRuntime> _runtime;

        public BootstrappingContext()
        {
            _runtime = new Lazy<JasperRuntime>(() => JasperRuntime.For(theRegistry));
            theRegistry.Services.For<ITransport>().Singleton().Add<StubTransport>();

            theRegistry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });
        }

        public void Dispose()
        {
            if (_runtime.IsValueCreated)
            {
                _runtime.Value.Dispose();
            }
        }

        public JasperRuntime theRuntime => _runtime.Value;

        public ChannelGraph theChannels => _runtime.Value.Container.GetInstance<ChannelGraph>();

        public StubTransport theTransport => _runtime.Value.Container.GetAllInstances<ITransport>()
            .OfType<StubTransport>()
            .Single();


    }
}
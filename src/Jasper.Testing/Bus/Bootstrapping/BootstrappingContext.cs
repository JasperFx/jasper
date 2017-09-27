using System;
using System.Linq;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Testing.Bus.Stubs;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class BootstrappingContext : IDisposable
    {
        public readonly Uri Uri1 = new Uri("stub://1");
        public readonly Uri Uri2 = new Uri("stub://2");
        public readonly Uri Uri3 = new Uri("stub://3");
        public readonly Uri Uri4 = new Uri("stub://4");

        public readonly JasperRegistry theRegistry = new JasperRegistry();

        private Lazy<JasperRuntime> _runtime;

        public BootstrappingContext()
        {
            _runtime = new Lazy<JasperRuntime>(() => JasperRuntime.For(theRegistry));
            theRegistry.Services.AddSingleton<ITransport, StubTransport>();

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

        public BusSettings theSettings => _runtime.Value.Get<BusSettings>();

        public JasperRuntime theRuntime => _runtime.Value;

        public IChannelGraph theChannels => _runtime.Value.Get<IChannelGraph>();

        public StubTransport theTransport => _runtime.Value.Container.GetAllInstances<ITransport>()
            .OfType<StubTransport>()
            .Single();

        public HandlerGraph theHandlers => _runtime.Value.Get<HandlerGraph>();
    }
}

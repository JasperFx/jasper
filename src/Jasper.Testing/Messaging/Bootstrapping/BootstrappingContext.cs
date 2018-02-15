using System;
using System.Linq;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Stub;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    [Collection("integration")]
    public class BootstrappingContext : IDisposable
    {
        private readonly Lazy<JasperRuntime> _runtime;

        public readonly JasperRegistry theRegistry = new JasperRegistry();
        public readonly Uri Uri1 = new Uri("stub://1");
        public readonly Uri Uri2 = new Uri("stub://2");
        public readonly Uri Uri3 = new Uri("stub://3");
        public readonly Uri Uri4 = new Uri("stub://4");

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

        public MessagingSettings theSettings => _runtime.Value.Get<MessagingSettings>();

        public JasperRuntime theRuntime => _runtime.Value;

        public IChannelGraph theChannels => _runtime.Value.Get<IChannelGraph>();

        public StubTransport theTransport => _runtime.Value.Get<ITransport[]>()
            .OfType<StubTransport>()
            .Single();

        public HandlerGraph theHandlers => _runtime.Value.Get<HandlerGraph>();

        public void Dispose()
        {
            if (_runtime.IsValueCreated)
                _runtime.Value.Dispose();
        }
    }
}

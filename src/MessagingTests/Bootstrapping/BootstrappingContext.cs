using System;
using Jasper;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Stub;
using Microsoft.Extensions.DependencyInjection;

namespace MessagingTests.Bootstrapping
{
    public class BootstrappingContext : IDisposable
    {
        public readonly JasperRegistry theRegistry = new JasperRegistry();
        public readonly Uri Uri1 = new Uri("stub://1");
        public readonly Uri Uri2 = new Uri("stub://2");
        public readonly Uri Uri3 = new Uri("stub://3");
        public readonly Uri Uri4 = new Uri("stub://4");
        private IJasperHost _host;

        public BootstrappingContext()
        {
            theRegistry.Services.AddSingleton<ITransport, StubTransport>();

            theRegistry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });
        }

        public void Dispose()
        {
            _host?.Dispose();
        }


        public IJasperHost theHost()
        {
            if (_host == null) _host = JasperHost.For(theRegistry);

            return _host;
        }

        public HandlerGraph theHandlers()
        {
            return (theHost()).Get<HandlerGraph>();
        }
    }
}

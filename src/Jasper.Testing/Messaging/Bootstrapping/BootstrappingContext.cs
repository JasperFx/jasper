using System;
using System.Threading.Tasks;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Stub;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    public class BootstrappingContext : IDisposable
    {
        public readonly JasperRegistry theRegistry = new JasperRegistry();
        public readonly Uri Uri1 = new Uri("stub://1");
        public readonly Uri Uri2 = new Uri("stub://2");
        public readonly Uri Uri3 = new Uri("stub://3");
        public readonly Uri Uri4 = new Uri("stub://4");
        private JasperRuntime _runtime;

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
            _runtime?.Dispose();
        }


        public async Task<JasperRuntime> theRuntime()
        {
            if (_runtime == null) _runtime = await JasperRuntime.ForAsync(theRegistry);

            return _runtime;
        }

        public async Task<HandlerGraph> theHandlers()
        {
            return (await theRuntime()).Get<HandlerGraph>();
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Stub;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    public class BootstrappingContext : IDisposable
    {
        private JasperRuntime _runtime;

        public readonly JasperRegistry theRegistry = new JasperRegistry();
        public readonly Uri Uri1 = new Uri("stub://1");
        public readonly Uri Uri2 = new Uri("stub://2");
        public readonly Uri Uri3 = new Uri("stub://3");
        public readonly Uri Uri4 = new Uri("stub://4");

        public BootstrappingContext()
        {
            theRegistry.Services.AddSingleton<ITransport, StubTransport>();

            theRegistry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });
        }


        public async Task<JasperRuntime> theRuntime()
        {
            if (_runtime == null)
            {
                _runtime = await JasperRuntime.ForAsync(theRegistry);
            }

            return _runtime;
        }

        public async Task<HandlerGraph> theHandlers()
        {
            return (await theRuntime()).Get<HandlerGraph>();
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }
    }
}

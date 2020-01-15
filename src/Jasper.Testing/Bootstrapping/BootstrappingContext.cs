using System;
using Jasper.Runtime.Handlers;
using Lamar;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Testing.Bootstrapping
{
    public class BootstrappingContext : IDisposable
    {
        public readonly JasperOptions theOptions = new JasperOptions();
        public readonly Uri Uri1 = new Uri("stub://1");
        public readonly Uri Uri2 = new Uri("stub://2");
        public readonly Uri Uri3 = new Uri("stub://3");
        public readonly Uri Uri4 = new Uri("stub://4");
        private IHost _host;

        public BootstrappingContext()
        {
            theOptions.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });
        }

        public void Dispose()
        {
            _host?.Dispose();
        }


        public IHost theHost()
        {
            if (_host == null) _host = JasperHost.For(theOptions);

            return _host;
        }

        public HandlerGraph theHandlers()
        {
            return (theHost()).Get<HandlerGraph>();
        }
    }
}

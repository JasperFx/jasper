using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Configuration
{
    public class registering_subscriptions_against_lazy_usage_of_config_key : IDisposable
    {
        private readonly JasperRegistry theRegistry = new JasperRegistry();
        private IDictionary<string, string> theConfigKeys = new Dictionary<string, string>();
        private IHost _host;


        private JasperOptions theOptions
        {
            get
            {
                if (_host == null)
                {
                    _host = Host.CreateDefaultBuilder()
                        .ConfigureAppConfiguration((c, builder) => builder.AddInMemoryCollection(theConfigKeys))
                        .UseJasper(theRegistry)
                        .Start();

                }

                return _host.Services.GetService<JasperOptions>();
            }
        }

        public void Dispose()
        {
            _host?.Dispose();
        }

    }
}

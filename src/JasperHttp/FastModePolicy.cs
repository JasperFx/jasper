using System;
using System.Linq;
using Baseline;
using Jasper;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace JasperHttp
{
    internal class FastModePolicy : IRegistrationPolicy
    {
        public void Apply(ServiceRegistry services)
        {

            services.RemoveAll(x =>
                x.ServiceType == typeof(IStartupFilter) &&
                x.ImplementationType == typeof(AutoRequestServicesStartupFilter));

            services.Insert(0, new ServiceDescriptor(typeof(IStartupFilter), typeof(ComplianceModeStartupFilter), ServiceLifetime.Singleton));
        }

        internal class ComplianceModeStartupFilter : IStartupFilter
        {
            private readonly JasperHttpOptions _options;

            public ComplianceModeStartupFilter(JasperHttpOptions options)
            {
                _options = options;
            }

            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                if (_options.AspNetCoreCompliance == ComplianceMode.GoFaster)
                {
                    return next;
                }

                return new AutoRequestServicesStartupFilter().Configure(next);
            }
        }
    }
}

using System;
using System.Reflection;
using Jasper;
using Jasper.Bus.Configuration;
using Jasper.Configuration;
using Jasper.Diagnostics;
using Jasper.Diagnostics.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly:JasperModule(typeof(DiagnosticsExtension))]

namespace Jasper.Diagnostics
{
    public class DiagnosticsExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Handlers.IncludeType(typeof(DiagnosticsHandler));
            registry.Logging.LogBusEventsWith<DiagnosticsBusLogger>();

            registry.Services.AddTransient<IStartupFilter, AddJasperDiagnosticMiddleware>();

            registry.Settings.Require<DiagnosticsSettings>();

            registry.Generation.Assemblies.Add(GetType().GetTypeInfo().Assembly);
        }
    }

    public class AddJasperDiagnosticMiddleware : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                var settings = builder.ApplicationServices.GetService<DiagnosticsSettings>();
                builder.UseDiagnostics(settings);
                next(builder);
            };
        }
    }
}



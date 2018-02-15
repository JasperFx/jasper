using System;
using System.Reflection;
using Jasper;
using Jasper.Configuration;
using Jasper.Diagnostics;
using Jasper.Diagnostics.Messages;
using Jasper.Messaging.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

// SAMPLE: UseJasperModule-with-Extension
[assembly:JasperModule(typeof(DiagnosticsExtension))]
// ENDSAMPLE

namespace Jasper.Diagnostics
{
    public class DiagnosticsExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Handlers.IncludeType(typeof(DiagnosticsHandler));
            registry.Logging.LogMessageEventsWith<DiagnosticsMessageLogger>();

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



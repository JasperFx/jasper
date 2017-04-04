using System;
using System.Threading.Tasks;
using Jasper.Codegen;
using Jasper.Configuration;
using JasperBus;
using StructureMap;

namespace Jasper.Diagnostics
{
    public class DiagnosticsFeature : IFeature
    {
        public readonly Registry Services = new DiagnosticServicesRegistry();

        private DiagnosticsServer _server;

        Task<Registry> IFeature.Bootstrap(JasperRegistry registry)
        {
            registry.Logging.LogBusEventsWith<DiagnosticsBusLogger>();
            return Task.FromResult(Services);
        }

        Task IFeature.Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            var settings = runtime.Registry.Settings.Get<DiagnosticsSettings>();

            return Task.Factory.StartNew(()=>
            {
                _server = new DiagnosticsServer();
                _server.Start(settings, runtime.Container);
            });
        }

        void IDisposable.Dispose()
        {
            _server?.Dispose();
        }
    }
}

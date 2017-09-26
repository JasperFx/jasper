using System;
using System.IO;
using System.Threading.Tasks;
using BlueMilk;
using BlueMilk.Codegen;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Configuration;
using StructureMap;

namespace Jasper.Diagnostics
{
    public class DiagnosticsFeature : IFeature
    {
        public readonly ServiceRegistry Services = new DiagnosticServicesRegistry();

        private DiagnosticsServer _server;

        Task<ServiceRegistry> IFeature.Bootstrap(JasperRegistry registry)
        {
            registry.Logging.LogBusEventsWith<DiagnosticsBusLogger>();
            return Task.FromResult(Services);
        }

        Task IFeature.Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            var settings = runtime.Get<DiagnosticsSettings>();

            return Task.Factory.StartNew(()=>
            {
                _server = new DiagnosticsServer();
                _server.Start(settings, runtime.Container);
            });
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            // TODO -- do more here, at least tell them the location
            writer.WriteLine("Diagostics are running...");
        }

        void IDisposable.Dispose()
        {
            _server?.Dispose();
        }
    }
}

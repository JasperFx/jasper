using System;
using System.Threading.Tasks;
using Jasper.Codegen;
using Jasper.Configuration;
using StructureMap;

namespace Jasper.Diagnostics
{
    public class DiagnosticsFeature : IFeature
    {
        public readonly Registry Services = new DiagnosticServicesRegistry();

        public string Url { get; set;} = "http://localhost:5200";

        private DiagnosticsServer _server;

        Task<Registry> IFeature.Bootstrap(JasperRegistry registry)
        {
            return Task.FromResult(Services);
        }

        Task IFeature.Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            return Task.Factory.StartNew(()=>
            {
                _server = new DiagnosticsServer();
                _server.Start(runtime.Container);
            });
        }

        void IDisposable.Dispose()
        {
            _server?.Dispose();
        }
    }
}

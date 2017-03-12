using System;
using System.Threading.Tasks;
using Jasper.Codegen;
using StructureMap;

namespace Jasper.Configuration
{
    public interface IFeature : IDisposable
    {
        Task<Registry> Bootstrap(JasperRegistry registry);
        Task Activate(JasperRuntime runtime, IGenerationConfig generation);
    }
}
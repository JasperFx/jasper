using System;
using System.IO;
using System.Threading.Tasks;
using BlueMilk.Codegen;
using StructureMap;

namespace Jasper.Configuration
{
    public interface IFeature : IDisposable
    {
        Task<Registry> Bootstrap(JasperRegistry registry);
        Task Activate(JasperRuntime runtime, IGenerationConfig generation);
        void Describe(JasperRuntime runtime, TextWriter writer);
    }
}

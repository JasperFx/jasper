using System;
using System.IO;
using System.Threading.Tasks;
using BlueMilk;
using BlueMilk.Codegen;
using Jasper.Util;

namespace Jasper.Configuration
{
    [Obsolete("Try to get rid of this")]
    public interface IFeature : IDisposable
    {
        Task<ServiceRegistry> Bootstrap(JasperRegistry registry, PerfTimer timer);
        void Activate(JasperRuntime runtime, GenerationRules generation, PerfTimer timer);
        void Describe(JasperRuntime runtime, TextWriter writer);
    }
}

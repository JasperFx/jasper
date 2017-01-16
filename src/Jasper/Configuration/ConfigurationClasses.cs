using System;
using System.Reflection;
using System.Threading.Tasks;
using StructureMap;


namespace Jasper.Configuration
{
    // Declare dependencies?
    public interface IJasperExtension
    {
        void Configure(JasperRegistry registry);
    }

    public interface IFeature : IDisposable
    {
        Task<Registry> Bootstrap(ConfigGraph graph);
        Task Activate(JasperRuntime runtime);
    }

    public class ConfigGraph
    {
        public Registry Registry { get; }
        public Assembly Assembly { get; }
        public Assembly[] Extensions { get; }

        public ConfigGraph(Registry registry, Assembly assembly, Assembly[] extensions)
        {
            Registry = registry;
            Assembly = assembly;
            Extensions = extensions;
        }

    }
}
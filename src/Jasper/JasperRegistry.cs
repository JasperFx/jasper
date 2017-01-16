using System.Reflection;
using Jasper.Configuration;
using StructureMap.TypeRules;

namespace Jasper
{
    public class JasperRegistry
    {
        public JasperRegistry()
        {
            var assembly = this.GetType().GetAssembly();
            if (assembly != typeof(JasperRegistry).GetAssembly())
            {
                ApplicationAssembly = assembly;
            }
        }

        public Assembly ApplicationAssembly { get; private set; }

        public ServiceRegistry Services { get; } = new ServiceRegistry();
    }
}
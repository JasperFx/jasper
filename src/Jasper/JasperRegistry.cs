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

        /// <summary>
        /// Convenience method to set the application assembly by using a Type
        /// contained in that Assembly
        /// </summary>
        /// <typeparam name="T">A type contained within the application assembly</typeparam>
        public void ApplicationContains<T>()
        {
            ApplicationAssembly = typeof(T).GetAssembly();
        }
    }
}
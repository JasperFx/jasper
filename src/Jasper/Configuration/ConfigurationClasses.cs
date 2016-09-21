using System;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;

namespace Jasper.Configuration
{
    public class JasperRuntime
    {
       
    }

    public interface IFeature
    {
        Task Bootstrap(ConfigGraph graph);
    }

    public class ConfigGraph
    {


        public ConfigGraph(Assembly assembly, Assembly[] extensions)
        {
        }

        public ServiceRegistry Services = new ServiceRegistry();
    }
}
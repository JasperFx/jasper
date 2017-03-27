using System.Reflection;
using Jasper.Configuration;
using Jasper.Settings;

namespace Jasper
{
    // TODO -- this only exists to create a readonly view of an application. Might eliminate
    // later or expose properties on JasperRuntime instead.
    public interface IJasperRegistry
    {
        T Feature<T>() where T : IFeature, new();
        IFeature[] Features { get; }
        Assembly ApplicationAssembly { get; }

        JasperSettings Settings { get; }
        ServiceRegistry Services { get; }
    }
}
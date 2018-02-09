using Baseline;
using Jasper.Configuration;

namespace Jasper.Http
{
    /// <summary>
    /// Base class for extending a JasperHttpRegistry based application
    /// </summary>
    public abstract class JasperHttpExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            Configure(registry.As<JasperHttpRegistry>());
        }

        public abstract void Configure(JasperHttpRegistry registry);
    }
}

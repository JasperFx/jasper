using System;
using Alba;
using Baseline;
using Jasper.Http.Routing;
using LamarCompiler.Frames;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.TestSupport.Alba
{
    /// <summary>
    /// Helper to quickly attach Alba scenario tests to a Jasper application
    /// </summary>
    public static class JasperAlba
    {
        public static SystemUnderTest ForBasic()
        {
            return For(new JasperRegistry());
        }


        public static SystemUnderTest For<T>(Action<T> overrides = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            overrides?.Invoke(registry);
            return For(registry);
        }

        /// <summary>
        /// Helper to quickly attach Alba scenario tests to a Jasper application
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static SystemUnderTest For(JasperRegistry registry)
        {
            var builder = registry.ToWebHostBuilder();
            var system = new SystemUnderTest(builder, registry.ApplicationAssembly);
            system.As<ISystemUnderTest>().Urls = new JasperUrlLookup(system.Services.GetRequiredService<IUrlRegistry>());

            return system;
        }
    }
}

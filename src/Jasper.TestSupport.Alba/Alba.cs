using Jasper;
using System;
using Alba;
using Baseline;
using Jasper.Configuration;
using JasperHttp.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly:JasperFeature]

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
            var builder = Host.CreateDefaultBuilder().UseJasper(registry);
            var system = new SystemUnderTest(builder, registry.ApplicationAssembly);
            system.As<ISystemUnderTest>().Urls = new JasperUrlLookup(system.Services.GetRequiredService<IUrlRegistry>());

            return system;
        }

        /// <summary>
        /// Build an Alba SystemUnderTest for a Jasper-enabled ASP.Net Core application
        /// with Jasper Url lookup
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static SystemUnderTest For(IWebHostBuilder builder)
        {
            builder.ConfigureServices(x => x.AddSingleton<IUrlLookup, JasperUrlLookup>());
            return new SystemUnderTest(builder);
        }

        /// <summary>
        /// Extension method that creates an Alba SystemUnderTest for an IWebHostBuilder
        /// with Jasper based Url lookup
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static SystemUnderTest ToAlbaSystem(this IWebHostBuilder builder)
        {
            return For(builder);
        }
    }
}

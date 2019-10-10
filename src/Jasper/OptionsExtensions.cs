using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jasper
{
    public static class OptionsExtensions
    {
        /// <summary>
        /// Add an IOptions for a type T and also forward it to be able to inject as just T
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sectionName"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddOptionsWithForwarding<T>(this IServiceCollection services, string sectionName = null) where T : class, new()
        {
            sectionName = sectionName ?? typeof(T).ConfigSectionName();
            services.AddOptions<T>(sectionName);
            services.AddSingleton<T>(s => s.GetService<IOptions<T>>().Value);
        }

        public static string ConfigSectionName(this Type type)
        {
            if (type.Name.EndsWith("Settings")) return type.Name.Substring(0, type.Name.Length - 8);
            if (type.Name.EndsWith("Options"))return type.Name.Substring(0, type.Name.Length - 7);

            return type.Name;
        }
    }
}

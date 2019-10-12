using System;
using Jasper;
using Jasper.Settings;

namespace JasperHttp
{
    public static class SettingsGraphExtensions
    {
        /// <summary>
        /// Convenience method to configure routing and action discovery setup for JasperHttp
        /// routes
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configure"></param>
        public static void Http(this SettingsGraph settings, Action<JasperHttpOptions> configure)
        {
            settings.Alter<JasperHttpOptions>(configure);
        }

        /// <summary>
        /// Convenience method to configure routing and action discovery setup for JasperHttp
        /// routes
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configure"></param>
        public static void Http(this JasperRegistry registry, Action<JasperHttpOptions> configure)
        {
            registry.Settings.Alter<JasperHttpOptions>(configure);
        }
    }
}

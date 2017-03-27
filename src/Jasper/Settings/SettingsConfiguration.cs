using System;
using Microsoft.Extensions.Configuration;

namespace Jasper.Settings
{
    public class SettingsConfiguration<T> : ISettingsConfiguration where T : class, new()
    {
        private readonly Func<IConfiguration, IConfiguration> _configuration;

        public SettingsConfiguration(Func<IConfiguration, IConfiguration> config)
        {
            _configuration = config;
        }

        public object Configure(IConfiguration configuration)
        {
            var result = new T();
            _configuration(configuration).Bind(result);
            return result;
        }
    }
}
using System;
using Microsoft.Extensions.Configuration;

namespace Jasper.Settings
{
    public class SettingsConfiguration<T> : ISettingsConfiguration where T : class, new()
    {
        private readonly Action<IConfiguration> _configuration;

        public SettingsConfiguration(Action<IConfiguration> config)
        {
            _configuration = config;
        }

        public object Configure(IConfiguration configuration)
        {
            var result = new T();
            _configuration(configuration);
            configuration.Bind(result);
            return result;
        }
    }
}
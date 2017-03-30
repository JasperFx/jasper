using System;
using Microsoft.Extensions.Configuration;

namespace Jasper.Settings
{
    public class SettingsConfiguration : ISettingsConfiguration
    {
        private readonly Func<IConfiguration, IConfiguration> _configuration;

        public SettingsConfiguration(Func<IConfiguration, IConfiguration> config)
        {
            _configuration = config;
        }

        public IConfiguration Configure(IConfiguration configuration)
        {
            return _configuration(configuration);
        }
    }
}
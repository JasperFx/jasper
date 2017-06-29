using System;
using Baseline;
using Jasper.Bus.Runtime;
using Microsoft.Extensions.Configuration;

namespace Jasper.Bus.Configuration
{
    // Only tested through integration tests
    public class ConfigUriLookup : IUriLookup
    {
        private readonly IConfigurationRoot _configuration;

        public ConfigUriLookup(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public string Protocol { get; } = "config";

        public Uri Lookup(Uri original)
        {
            var key = original.Host;

            var uriString = _configuration.GetValue<string>(key);

            if (uriString.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(original), $"Could not find a configuration value for '{key}'");
            }

            try
            {
                return uriString.ToUri();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Could not parse '{uriString}' from configuration item {key} into a Uri");
            }
        }
    }
}

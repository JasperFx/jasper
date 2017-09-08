using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Util;
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

        public Task<Uri[]> Lookup(Uri[] originals)
        {
            var actuals = new Uri[originals.Length];
            for (int i = 0; i < originals.Length; i++)
            {
                var original = originals[i];

                var key = original.Host;

                var uriString = _configuration.GetValue<string>(key);

                if (uriString.IsEmpty())
                {
                    throw new ArgumentOutOfRangeException(nameof(originals), $"Could not find a configuration value for '{key}'");
                }

                try
                {
                    actuals[i] = uriString.ToUri();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Could not parse '{uriString}' from configuration item {key} into a Uri");
                }
            }

            return Task.FromResult(actuals);

        }
    }
}

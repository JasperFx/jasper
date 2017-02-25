using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace Jasper.Binding
{
    public interface IDataSource
    {
        bool Has(string key);
        string Get(string key);

        bool HasChild(string key);

        IDataSource GetChild(string key);
    }

    public class ConfigurationProviderDataSource : IDataSource
    {
        private readonly IConfigurationProvider _provider;

        public ConfigurationProviderDataSource(IConfigurationProvider provider)
        {
            _provider = provider;
            provider.Load();
        }

        public bool Has(string key)
        {
            string value = null;
            return _provider.TryGet(key, out value);
        }

        public string Get(string key)
        {
            string value = null;
            _provider.TryGet(key, out value);

            return value;
        }

        public bool HasChild(string key)
        {
            return _provider.
            throw new System.NotImplementedException();
        }

        public IDataSource GetChild(string key)
        {
            throw new System.NotImplementedException();
        }
    }
}
using System;
using Baseline;
using Microsoft.Extensions.Configuration;

namespace Jasper.Settings
{
    public class SettingsProvider : ISettingsProvider
    {
        private readonly IConfigurationRoot _root;

        public SettingsProvider(IConfigurationRoot root)
        {
            _root = root;
        }

        public T Get<T>() where T : class, new()
        {
            return _root.Get<T>() ?? Activator.CreateInstance(typeof(T)).As<T>();
        }


    }
}

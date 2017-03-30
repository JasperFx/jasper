using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Configuration;

namespace Jasper.Settings
{
    public class RegistryAlteration<T> : IRegistryAlteration where T : class, new() 
    {
        private readonly Action<T> _alteration;

        public RegistryAlteration(Action<T> alteration)
        {
            _alteration = alteration;
        }

        public void Alter(SettingsProvider settings)
        {
            settings.AlterRegistry(_alteration);
        }
    }
}

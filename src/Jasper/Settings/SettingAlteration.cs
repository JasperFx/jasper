using System;

namespace Jasper.Settings
{
    public class SettingAlteration<T> : ISettingsAlteration where T : class, new()
    {
        private readonly Action<T> _alteration;

        public SettingAlteration(Action<T> alteration)
        {
            _alteration = alteration;
        }

        public void Alter(SettingsCollection settings)
        {
            settings.Alter(_alteration);
        }
    }
}
using System;

namespace Jasper.Settings
{
    public class SettingReplacement<T> : ISettingsAlteration where T : class
    {
        private readonly T _settings;

        public SettingReplacement(T settings)
        {
            _settings = settings;
        }

        public void Alter(SettingsCollection settings)
        {
            settings.Replace(_settings);
        }
    }
}
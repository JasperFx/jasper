namespace Jasper.Settings
{
    public interface IRegistryAlteration
    {
        void Alter(SettingsProvider settingsProvider);
    }
}
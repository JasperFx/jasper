namespace Jasper.Settings
{
    public interface ISettingsAlteration
    {
        void Alter(SettingsCollection settings);
    }
}
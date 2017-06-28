namespace Jasper.Settings
{
    public interface ISettingsProvider
    {
        T Get<T>() where T : class, new();
    }
}
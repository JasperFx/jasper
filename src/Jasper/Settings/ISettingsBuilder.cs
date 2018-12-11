using Lamar;

namespace Jasper.Settings
{
    public interface ISettingsBuilder
    {
        void Apply(ServiceRegistry services);
    }
}
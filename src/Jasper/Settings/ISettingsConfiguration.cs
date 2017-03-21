using Microsoft.Extensions.Configuration;

namespace Jasper.Settings
{
    public interface ISettingsConfiguration
    {
        object Configure(IConfiguration configuration);
    }
}
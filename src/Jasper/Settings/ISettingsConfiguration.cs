using Microsoft.Extensions.Configuration;

namespace Jasper.Settings
{
    public interface ISettingsConfiguration
    {
        IConfiguration Configure(IConfiguration configuration);
    }
}
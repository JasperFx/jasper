using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace Jasper.Diagnostics.StructureMap
{
    public static class ServiceCollectionExtensions
    {
        public static IWebHostBuilder UseStructureMap(this IWebHostBuilder builder, IContainer container)
        {
            return builder.ConfigureServices(services => services.AddStructureMap(container));
        }

        public static IServiceCollection AddStructureMap(this IServiceCollection services, IContainer container)
        {
            return services.AddSingleton<IServiceProviderFactory<IContainer>>(new StructureMapServiceProviderFactory(container));
        }
    }
}

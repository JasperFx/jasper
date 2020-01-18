#if NETSTANDARD2_0
using System.Threading.Tasks;
namespace Microsoft.AspNetCore.Hosting
{
    public static class IWebHostBuilderExtentions
    {
        public static async Task<IWebHost> StartAsync(this IWebHostBuilder builder)
        {
            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
#endif

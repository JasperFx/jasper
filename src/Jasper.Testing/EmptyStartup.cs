using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Testing
{
    public class EmptyStartup
    {
        public void ConfigureServices(IServiceCollection services){}

        public void Configure(IApplicationBuilder app){}
    }
}
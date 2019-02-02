using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TestingSupport
{
    public class EmptyStartup
    {
        public void ConfigureServices(IServiceCollection services){}

        public void Configure(IApplicationBuilder app){}
    }
}

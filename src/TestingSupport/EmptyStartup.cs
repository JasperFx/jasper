using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTests
{
    public class EmptyStartup
    {
        public void ConfigureServices(IServiceCollection services){}

        public void Configure(IApplicationBuilder app){}
    }
}

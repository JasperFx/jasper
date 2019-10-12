using System;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Configuration
{
    internal class NulloStartup : IStartup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return new Container(services);
        }

        public void Configure(IApplicationBuilder app)
        {
            Console.WriteLine("Jasper 'Nullo' startup is being used to start the ASP.Net Core application");
        }
    }
}

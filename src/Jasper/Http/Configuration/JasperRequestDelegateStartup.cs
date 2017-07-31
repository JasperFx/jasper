using System;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http.Configuration
{
    internal class JasperRequestDelegateStartup : IStartup
    {
        private readonly Router _router;

        public JasperRequestDelegateStartup(Router router)
        {
            _router = router;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // TODO -- sounds goofy, but this never gets used
            return null;
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Use(_router.Apply);
        }
    }
}
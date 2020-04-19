using System;
using System.Linq;
using Lamar;
using LamarCodeGeneration.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Http
{
    public static class EndpointBuilderExtensions
    {
        // TODO -- do something to let you chain routing configuration
        // here later

        // TODO -- might want an option to add IConfiguration or Hosting options too
        /// <summary>
        /// Add Jasper HTTP endpoint routes
        /// </summary>
        /// <param name="builder"></param>
        public static void MapJasperEndpoints(this IEndpointRouteBuilder builder, Action<JasperHttpOptions> configure = null)
        {
            builder.DataSources.Add(new JasperRouteEndpointSource((IContainer) builder.ServiceProvider, configure));
        }


        /// <summary>
        /// Configure the Jasper HTTP routing
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static void ConfigureHttp(this IExtensions extensions, Action<JasperHttpOptions> configure)
        {
            var extension = extensions.GetRegisteredExtension<JasperHttpExtension>();

            if (extension == null)
            {
                extensions.Include<JasperHttpExtension>();
                extension = extensions.GetRegisteredExtension<JasperHttpExtension>();
            }

            configure(extension.Options);
        }

        internal class JasperHttpStartup : IStartup
        {
            public Action<JasperHttpOptions> Customization { get; set; }

            public void Configure(IApplicationBuilder app)
            {
                app.UseRouting();
                app.UseEndpoints(x => x.MapJasperEndpoints(Customization));
            }

            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                return new Container(services);
            }
        }
    }
}

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jasper.Diagnostics;
using Jasper.Remotes.Messaging;

namespace DiagnosticsHarness
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly Func<HttpContext, bool> isApiRequest = context => context.Request.Path.Value.StartsWith("/api");

        public Startup(IHostingEnvironment env)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddDiagnostics();

            services.AddMvc();

            services.AddCors(options =>
            {
                options.AddPolicy("OriginPolicy", builder => builder.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod());
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors("OriginPolicy");

            app.UseWebSockets();

            app.UseDiagnostics(_ =>
            {
                _.Mode = DiagnosticsMode.Development;
            });

            UseRequestLogging(app);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public static void UseRequestLogging(IApplicationBuilder app)
        {
            var client = app.ApplicationServices.GetService<IDiagnosticsClient>();

             app.Use( async (context, next) =>
             {
                 client.Send(new LogMessage($"Incoming request: {context.Request.Method}, {context.Request.Path}, {context.Request.Headers}"));

                 await next();

                 client.Send(new LogMessage($"Outgoing response: {context.Response.StatusCode} {context.Response.Headers}"));
             });
         }

         public class LogMessage : ClientMessage
         {
             public LogMessage(string message) : base("log")
             {
                 Message = message;
             }

             public string Message { get; }
         }
    }
}

using System;
using System.Threading.Tasks;
using DiagnosticsHarnessMessages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jasper;
using Jasper.Bus;
using Jasper.Diagnostics;

namespace DiagnosticsHarness
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

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

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddCors(options =>
            {
                options.AddPolicy("OriginPolicy", builder => builder.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod());
            });

            var runtime = JasperRuntime.For<BusRegistry>(_ =>
            {
                _.Services.AddRange(services);
                _.Services.ForSingletonOf<IConfigurationRoot>().Use(Configuration);
            });

            return runtime.Get<IServiceProvider>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors("OriginPolicy");

            app.UseDiagnostics(_ =>
            {
//                _.AuthorizeWith = context => context.User.HasClaim("admin", "true");
            });

            UseRequestLogging(app);
            UseErrorMap(app);
        }

        public static void UseRequestLogging(IApplicationBuilder app)
        {
            var bus = app.ApplicationServices.GetService<IServiceBus>();

             app.Use( async (context, next) =>
             {
                 await bus.Send(new MiddlewareMessage { Message = $"Incoming request: {context.Request.Method}, {context.Request.Path}, {context.Request.Headers}" });

                 await next();

                 await bus.Send(new MiddlewareMessage { Message = $"Outgoing response: {context.Response.StatusCode} {context.Response.Headers}" });
             });
         }

         public static void UseErrorMap(IApplicationBuilder app)
         {
             var bus = app.ApplicationServices.GetService<IServiceBus>();

             app.Map("/error", _ => _.Use( async (context, next) => {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("error endpoint");

                await bus.Send(new AMessageThatWillError());
             }));
         }

         public class BusRegistry : JasperRegistry
         {
             public BusRegistry()
             {
                 var uri = "durable://localhost:2110/servicebus_example";
                 Publish.Message<MiddlewareMessage>().To(uri);
                 Publish.Message<AMessageThatWillError>().To(uri);

                 Logging.UseConsoleLogging = true;

             }
         }
    }

    public class SomeMiddleware
    {
        private readonly IServiceBus _bus;

        public SomeMiddleware(RequestDelegate next, IServiceBus bus)
        {
            _bus = bus;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("error endpoint");

            await _bus.Send(new AMessageThatWillError());
        }
    }
}

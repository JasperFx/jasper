using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jasper;
using Jasper.Diagnostics;
using JasperBus;
using StructureMap;

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
                _.Services.Populate(services);
                _.Services.ForSingletonOf<IConfigurationRoot>().Use(Configuration);
                // x.Policies.OnMissingFamily<SettingsPolicy>();
            });

            return runtime.Container.GetInstance<IServiceProvider>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors("OriginPolicy");

            app.UseDiagnostics(_ =>
            {
                _.Mode = DiagnosticsMode.Development;
                // _.AuthorizeWith = context => context.User.HasClaim("admin", "true");
            });

            UseRequestLogging(app);
            UseErrorMap(app);
        }

        public static void UseRequestLogging(IApplicationBuilder app)
        {
            var bus = app.ApplicationServices.GetService<IServiceBus>();

             app.Use( async (context, next) =>
             {
                 bus.Send(new MiddlewareMessage { Message = $"Incoming request: {context.Request.Method}, {context.Request.Path}, {context.Request.Headers}" });

                 await next();

                 bus.Send(new MiddlewareMessage { Message = $"Outgoing response: {context.Response.StatusCode} {context.Response.Headers}" });
             });
         }

         public static void UseErrorMap(IApplicationBuilder app)
         {
             var bus = app.ApplicationServices.GetService<IServiceBus>();

             app.Map("/error", _ => _.Use( async (context, next) => {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("error endpoint");

                bus.Send(new AMessageThatWillError());
             }));
         }

         public class BusRegistry : JasperBusRegistry
         {
             public BusRegistry()
             {
                 var uri = "lq.tcp://localhost:2110/servicebus_example";
                 SendMessage<MiddlewareMessage>().To(uri);
                 SendMessage<AMessageThatWillError>().To(uri);
                 ListenForMessagesFrom(uri);

                 Logging.UseConsoleLogging = true;

                 this.AddDiagnostics();
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

            _bus.Send(new AMessageThatWillError());
        }
    }

    public class MiddlewareMessage
    {
        public string Message { get; set; }
    }

    public class MiddlewareMessageConsumer
    {
        public void Consume(MiddlewareMessage message)
        {
            Console.WriteLine($"Got Message: {message.Message}");
        }
    }

    public class AMessageThatWillError
    {
    }

    public class SomeConsumer
    {
        public void Consume(AMessageThatWillError message)
        {
            throw new NotSupportedException();
        }
    }
}

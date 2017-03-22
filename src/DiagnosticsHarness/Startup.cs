using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jasper.Diagnostics;
using Jasper.Remotes.Messaging;
using Jasper;
using StructureMap;
using JasperBus;

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
            });

            UseRequestLogging(app);
        }

        public static void UseRequestLogging(IApplicationBuilder app)
        {
            var client = app.ApplicationServices.GetService<IDiagnosticsClient>();
            var bus = app.ApplicationServices.GetService<IServiceBus>();

             app.Use( async (context, next) =>
             {
                 bus.Send(new MiddlewareMessage { Message = $"Incoming request: {context.Request.Method}, {context.Request.Path}, {context.Request.Headers}" });

                 await next();

                 client.Send(new MiddlewareMessage { Message = $"Outgoing response: {context.Response.StatusCode} {context.Response.Headers}" });
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

         public class BusRegistry : JasperBusRegistry
         {
             public BusRegistry()
             {
                 var uri = "lq.tcp://localhost:2110/servicebus_auth";
                 SendMessage<MiddlewareMessage>().To(uri);
                 ListenForMessagesFrom(uri);

                 Logging.UseConsoleLogging = true;

                 this.AddDiagnostics();
             }
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
}

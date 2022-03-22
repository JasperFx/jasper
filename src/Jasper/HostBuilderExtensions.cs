using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Runtime;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton;
using Oakton.Descriptions;
using OpenTelemetry.Trace;

namespace Jasper
{
    public static class HostBuilderExtensions
    {

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application using a custom JasperOptionsBuilder (or JasperOptions) type
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IHostBuilder UseJasper<T>(this IHostBuilder builder, Action<HostBuilderContext, JasperOptions>? overrides = null)
            where T : JasperOptions, new()
        {
            return builder.UseJasper(new T(), overrides);
        }

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application with optional configuration to Jasper
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides">Programmatically configure Jasper options</param>
        /// <returns></returns>
        public static IHostBuilder UseJasper(this IHostBuilder builder, Action<HostBuilderContext, JasperOptions>? overrides = null)
        {

            var registry = new JasperOptions();

            return builder.UseJasper(registry, overrides);
        }

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application with optional configuration to Jasper
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides">Programmatically configure Jasper options</param>
        /// <returns></returns>
        public static IHostBuilder UseJasper(this IHostBuilder builder, Action<JasperOptions> overrides)
        {
            return builder.UseJasper(new JasperOptions(), (c, r) => overrides(r));
        }

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application with a pre-built JasperOptionsBuilder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionsy"></param>
        /// <returns></returns>
        public static IHostBuilder UseJasper(this IHostBuilder builder, JasperOptions options, Action<HostBuilderContext, JasperOptions>? customization = null)
        {
            var appliedKey = "JASPER_HAS_BEEN_APPLIED";
            if (builder.Properties.ContainsKey(appliedKey))
                throw new InvalidOperationException($"{nameof(UseJasper)} can only be called once per builder");

            builder.Properties.Add(appliedKey, "true");

            ExtensionLoader.ApplyExtensions(options);

            builder.UseLamar();

            builder.ConfigureServices((context, services) =>
            {
                options.Configure(context.HostingEnvironment, context.Configuration);

                customization?.Invoke(context, options);

                options.HandlerGraph.StartCompiling(options);

                services.AddSingleton<IDescribedSystemPart>(options.HandlerGraph);
                services.AddSingleton<IDescribedSystemPart>(options.Transports);

                services.AddSingleton(options);

                // The messaging root is also a hosted service
                services.AddSingleton(s => (IHostedService)s.GetService<IMessagingRoot>());

                services.AddRange(options.CombineServices());
            });

            return builder;
        }


        /// <summary>
        ///     Syntactical sugar to execute the Jasper command line for a configured WebHostBuilder
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Task<int> RunJasper(this IHostBuilder hostBuilder, string[] args)
        {
            return hostBuilder.RunOaktonCommands(args);
        }

        public static T Get<T>(this IHost host)
        {
            return host.Services.As<IContainer>().GetInstance<T>();
        }

        public static object Get(this IHost host, Type serviceType)
        {
            return host.Services.GetRequiredService(serviceType);
        }

        /// <summary>
        /// Syntactical sugar for host.Services.GetRequiredService<IMessagePublisher>().Send(message)
        /// </summary>
        /// <param name="host"></param>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task Send<T>(this IHost host, T? message)
        {
            return host.Get<IMessagePublisher>().SendAsync(message);
        }

        public static Task Invoke<T>(this IHost host, T? command)
        {
            return host.Get<ICommandBus>().Invoke(command);
        }

        /// <summary>
        /// Add Jasper tracing to your Open Telemetry diagnostic publishing
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static TracerProviderBuilder AddJasper(this TracerProviderBuilder builder)
        {

            return builder.AddSource("Jasper");
        }
    }
}

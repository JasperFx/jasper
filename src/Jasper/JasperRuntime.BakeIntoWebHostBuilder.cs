using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.EnvironmentChecks;
using Jasper.Http;
using Jasper.Messaging.Transports.Configuration;
using Lamar;
using Lamar.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using TypeExtensions = Baseline.TypeExtensions;

namespace Jasper
{
    public partial class JasperRuntime
    {
        public static void BakeIntoWebHostBuilder(IWebHostBuilder builder, JasperRegistry registry)
        {
            var timer = new PerfTimer();
            timer.Start("Bootstrapping");

            timer.Record("Finding and Applying Extensions", () =>
            {
                applyExtensions(registry);
            });

            var handlerCompilation = registry.Messaging.CompileHandlers(registry, timer);



            var runtime = new JasperRuntime(registry, timer);


            registry.CopyRegistrations(builder);



            var startup = new LightweightJasperStartup(runtime, timer, handlerCompilation);
            builder.ConfigureServices(s => s.AddSingleton<IStartup>(startup));

            timer.Stop();
        }


        public class LightweightJasperStartup : IStartup
        {
            public static IStartup Build(IServiceProvider provider, ServiceDescriptor descriptor)
            {
                if (descriptor.ImplementationInstance != null)
                {
                    return TypeExtensions.As<IStartup>(descriptor.ImplementationInstance);
                }

                if (descriptor.ImplementationType != null)
                {
                    return TypeExtensions.As<IStartup>(provider.GetService(descriptor.ServiceType));
                }

                return TypeExtensions.As<IStartup>(descriptor.ImplementationFactory(provider));
            }



            private readonly JasperRuntime _runtime;
            private readonly PerfTimer _timer;
            private readonly Task _handlerCompilation;
            private IStartup[] _startups;


            public LightweightJasperStartup(JasperRuntime runtime, PerfTimer timer, Task handlerCompilation)
            {
                _runtime = runtime;
                _timer = timer;
                _handlerCompilation = handlerCompilation;
            }

            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                var others = services
                    .Where(x => x.ServiceType == typeof(IStartup))
                    .Where(x => x.ImplementationInstance != this).ToArray();

                services.RemoveAll(s => others.Contains(s));

                var combined = _runtime.Registry.CombineServices(services);

                combined.AddSingleton(_runtime);

                // TODO -- need to pass in the perf timer here
                _runtime.Container = new Container(combined);

                _startups = others.Select(x => Build(_runtime.Container, x)).ToArray();

                var additional = new ServiceCollection();

                // I know this is goofy as all hell, but there is code
                // in MVC that tries to pick things out of the service collection
                // during bootstrapping
                additional.AddRange(services);


                foreach (var startup in _startups)
                {
                    startup.ConfigureServices(additional);
                }

                // See the rant-y comment above
                additional.RemoveAll(services.Contains);

                _runtime.Container.Configure(additional);

                if (!_runtime.Container.Model.HasRegistrationFor<IServer>())
                {
                    _runtime.Container.Configure(x => x.AddSingleton<IServer>(new NulloServer()));
                }

                if (_runtime.Registry.HttpRoutes.Enabled)
                {
                    _runtime
                        .Registry
                        .HttpRoutes
                        .FindRoutes(_runtime, _runtime.Registry, _timer)
                        .GetAwaiter()
                        .GetResult();
                }

                _handlerCompilation.GetAwaiter().GetResult();


                // Run environment checks
                _timer.Record("Environment Checks", () =>
                {
                    var recorder = EnvironmentChecker.ExecuteAll(_runtime);
                    if (_runtime.Get<MessagingSettings>().ThrowOnValidationErrors) recorder.AssertAllSuccessful();
                });

                _timer.Stop();

                return _runtime.Container;
            }

            public void Configure(IApplicationBuilder app)
            {
                var router = _runtime.Registry.HttpRoutes.Routes.Router;
                app.StoreRouter(router);

                foreach (var startup in _startups)
                {
                    startup.Configure(app);
                }

                if (!app.HasJasperBeenApplied() && router.HasAnyRoutes())
                {
                    app.Run(router.Invoke);
                }
            }
        }
    }



}

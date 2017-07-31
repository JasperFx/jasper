using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Http.Configuration;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace Jasper.Http
{


    public class AspNetCoreFeature : IFeature
    {
        public readonly ActionSource Actions = new ActionSource();

        public readonly RouteGraph Routes = new RouteGraph();

        private readonly HostBuilder _builder;
        private readonly Registry _services;
        private IWebHost _host;

        public AspNetCoreFeature()
        {
            Actions.IncludeClassesSuffixedWithEndpoint();

            _services = new Registry();
            _builder = new HostBuilder(this);
            _builder.ConfigureServices(_ =>
            {
                _.Add(new ServiceDescriptor(typeof(Router), Routes.Router));
                _.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            });
        }

        public string EnvironmentName
        {
            get => _builder.GetSetting(WebHostDefaults.EnvironmentKey);
            set => _builder.UseEnvironment(value);
        }


        public IWebHostBuilder WebHostBuilder => _builder;

        internal bool BootstrappedWithinAspNetCore { get; set; }

        public void Dispose()
        {
            _host?.Dispose();
        }

        public async Task<Registry> Bootstrap(JasperRegistry registry)
        {
            var actions = await Actions.FindActions(registry.ApplicationAssembly);
            foreach (var methodCall in actions)
            {
                Routes.AddRoute(methodCall);
            }

            _services.For<RouteGraph>().Use(Routes);
            _services.For<IUrlRegistry>().Use(Routes.Router.Urls);

            if (!BootstrappedWithinAspNetCore)
            {
                _services.For<IServer>().Add<NulloServer>().Singleton();
            }

            return _services;
        }

        public Task Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            return Task.Factory.StartNew(() =>
            {
                var rules = runtime.Get<ConnegRules>();

                Routes.BuildRoutingTree(rules, generation, runtime.Container);

                if (!BootstrappedWithinAspNetCore)
                {
                    _host = _builder.Activate(runtime.Container, Routes.Router);

                    runtime.Container.Inject(_host);

                    _host.Start();
                }
            });
        }
    }

}

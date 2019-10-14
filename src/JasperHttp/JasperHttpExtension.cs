using Baseline;
using Baseline.Reflection;
using Jasper;
using Jasper.Configuration;
using Jasper.Conneg;
using JasperHttp.ContentHandling;
using JasperHttp.MVCExtensions;
using JasperHttp.Routing;
using Lamar;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JasperHttp
{
    public class JasperHttpExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            var options = new JasperHttpOptions();
            registry.Settings.Replace(options);

            registry.Services.ForConcreteType<ConnegRules>().Configure.Singleton();
            registry.Services.For<IHttpContextAccessor>().Use(x => new HttpContextAccessor());
            registry.Services.AddSingleton(options.Routes);

            registry.Services.ForSingletonOf<IUrlRegistry>().Use(options.Urls);


            registry.Services.Policies.Add(new RouteScopingPolicy(options.Routes));

            // This guarantees that the Jasper middleware is part of the RequestDelegate
            // at the end if it has not been explicitly added
            registry.Services.AddSingleton<IStartupFilter>(new RegisterJasperStartupFilter(options));

            options.ApplicationAssembly = registry.ApplicationAssembly;

            // SAMPLE: applying-route-policy
            // Applying a global policy
            options.GlobalPolicy<ControllerUsagePolicy>();

            options.IncludeTypes(x => x.CanBeCastTo<ControllerBase>());
            options.IncludeMethods(x => x.HasAttribute<HttpMethodAttribute>());

            registry.Services.Scan(x =>
            {
                x.AssemblyContainingType<JasperHttpExtension>();
                x.ConnectImplementationsToTypesClosing(typeof(ISerializerFactory<,>));

            });

            registry.Services.Scan(x =>
            {
                x.Assembly(registry.ApplicationAssembly);
                x.AddAllTypesOf<IRequestReader>();
                x.AddAllTypesOf<IResponseWriter>();
            });
            // ENDSAMPLE



            registry.Services.AddSingleton<IWriterRule, ActionResultWriterRule>();

            RouteBuilder.PatternRules.Insert(0, new HttpAttributePatternRule());

            // TODO -- might need to bring this back for tests
            /*
                // Registers an empty startup if there is none in the application
                if (s.All(x => x.ServiceType != typeof(IStartup))) s.AddSingleton(new NulloStartup());

                // Registers a "nullo" server if there is none in the application
                // i.e., Kestrel isn't applied
                if (s.All(x => x.ServiceType != typeof(IServer))) s.AddSingleton(new NulloServer());
             */
        }
    }
}

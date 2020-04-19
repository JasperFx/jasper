using Baseline;
using Baseline.Reflection;
using Jasper.Http.ContentHandling;
using Jasper.Http.MVCExtensions;
using Jasper.Http.Routing;
using Jasper.Serialization;
using Lamar;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    public class JasperHttpExtension : IJasperExtension
    {
        public void Configure(JasperOptions registry)
        {
            registry.Services.AddSingleton(Options);


            registry.Services.ForConcreteType<ConnegRules>().Configure.Singleton();
            registry.Services.AddSingleton(Options.Routes);

            registry.Services.ForSingletonOf<IUrlRegistry>().Use(Options.Urls);


            registry.Services.Policies.Add(new RouteScopingPolicy(Options.Routes));

            // SAMPLE: applying-route-policy
            // Applying a global policy
            Options.GlobalPolicy<ControllerUsagePolicy>();

            Options.IncludeTypes(x => x.CanBeCastTo<ControllerBase>());
            Options.IncludeMethods(x => x.HasAttribute<HttpMethodAttribute>());

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

            JasperRoute.Rules.Insert(0, new HttpAttributeRoutingRule());
        }

        public JasperHttpOptions Options { get; } = new JasperHttpOptions();
    }
}

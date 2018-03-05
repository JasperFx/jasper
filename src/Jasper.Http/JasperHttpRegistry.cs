using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http.ContentHandling;
using Jasper.Http.Routing;
using Jasper.Http.Transport;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports;
using Lamar.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    public class JasperHttpRegistry : JasperRegistry
    {
        public JasperHttpRegistry()
        {
            Settings.Replace(Http.Settings);
            Settings.Replace(Http.Transport.As<HttpTransportSettings>()); // Hokey, but I'll allow it

            _baseServices.For<ITransport>()
                .Use<HttpTransport>();


            _baseServices.AddSingleton<ConnegRules>();

            _baseServices.AddScoped<IHttpContextAccessor>(x => new HttpContextAccessor());
            _baseServices.AddSingleton(Http.Routes.Router);
            _baseServices.AddSingleton(Http.Routes);
            _baseServices.ForSingletonOf<IUrlRegistry>().Use(Http.Routes.Router.Urls);

            _baseServices.AddSingleton(x => Http.Host);
        }

        /// <summary>
        ///     IWebHostBuilder and other configuration for ASP.net Core usage within a Jasper
        ///     application
        /// </summary>
        public AspNetCoreFeature Http { get; } = new AspNetCoreFeature();


        /// <summary>
        ///     Gets or sets the ASP.Net Core environment names
        /// </summary>
        public override string EnvironmentName
        {
            get => Http.EnvironmentName;
            set => Http.EnvironmentName = value;
        }

        protected override string HttpAddresses => Http.As<IWebHostBuilder>().GetSetting(WebHostDefaults.ServerUrlsKey);

        protected override Task BuildFeatures(JasperRuntime runtime, PerfTimer timer)
        {
            return Http.FindRoutes(runtime, this, timer);
        }

        protected override void Describe(JasperRuntime runtime, TextWriter writer)
        {
            base.Describe(runtime, writer);
            Http.Describe(runtime, writer);
        }

        protected override void AlterNode(ServiceNode local)
        {
            local.MessagesUrl = Http.Transport.As<HttpTransportSettings>().RelativeUrl;
        }

        protected override Task Startup(JasperRuntime runtime)
        {
            // Handled by ASP.Net Core itself
            return Task.CompletedTask;
        }


        protected override Task Stop(JasperRuntime runtime)
        {
            // Handled by ASP.Net Core itself
            return Task.CompletedTask;
        }
    }
}

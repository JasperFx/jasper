using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders.Embedded;
using Baseline;
using JasperBus;
using JasperBus.Model;
using Jasper.Diagnostics.Util;
using Jasper.Remotes.Messaging;
using StructureMap;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Diagnostics
{
    public class DiagnosticsOptions
    {
        public string PageTitle { get; set; } = "Jasper Diagnostics";
        public string Mode { get; set; } = DiagnosticsMode.Production;
        public PathString BasePath { get; set; } = "/_diag";
        public int WebsocketPort { get; set; } = 5000;
    }

    public class DiagnosticsMode
    {
        public static readonly string Development = "Development";
        public static readonly string Production = "Production";
    }

    public class DiagnosticsMiddleware
    {
        public const string Resource_Root = "/_res";
        public const string Bundle_Name = "bundle.js";

        private readonly RequestDelegate _next;
        private readonly DiagnosticsOptions _options;

        public DiagnosticsMiddleware(RequestDelegate next, DiagnosticsOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest ||
             !context.Request.Path.StartsWithSegments(_options.BasePath))
            {
                await _next(context);
                return;
            }

            await buildLandingPage(context);
        }

        private async Task buildLandingPage(HttpContext context)
        {
            var document = new HtmlDocument { Title = _options.PageTitle };

            document.Head.Append(new CssTag("https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css"));

            document.Body.Append(new HtmlTag("div").Attr("id", "root"));

            var websocketUri = $"ws://localhost:{_options.WebsocketPort}{_options.BasePath}/ws";

            var initialData = new HtmlTag("script")
                .Attr("type", "text/javascript")
                .Text("{ var DiagnosticsSettings = { websocketAddress: '" + websocketUri + "' } }");

            document.Body.Append(initialData);

            if(_options.Mode == DiagnosticsMode.Development)
            {
                document.Body.Append(new ScriptTag($"/{Bundle_Name}"));
            }
            else
            {
                document.Body.Append(new ScriptTag($"{_options.BasePath}{Resource_Root}/js/{Bundle_Name}"));
            }

            await context.WriteHtml(document);
        }
    }

    public static class DiagnosticsMiddlewareExtensions
    {
        public static IApplicationBuilder UseDiagnostics(
            this IApplicationBuilder app,
            Action<DiagnosticsOptions> configure = null)
        {
            var options = new DiagnosticsOptions();
            configure?.Invoke(options);
            return UseDiagnostics(app, options);
        }

        public static IApplicationBuilder UseDiagnostics(
            this IApplicationBuilder app,
            DiagnosticsOptions options)
        {
            app.UseWebSockets();

            if(options.Mode == DiagnosticsMode.Production)
            {
                var assembly = typeof(DiagnosticsMiddleware).GetTypeInfo().Assembly;
                var provider = new EmbeddedFileProvider(assembly, $"{assembly.GetName().Name}.resources");

                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = provider,
                    RequestPath = new PathString($"{options.BasePath}{DiagnosticsMiddleware.Resource_Root}")
                });
            }

            var hub = app.ApplicationServices.GetService<IMessagingHub>();
            var manager = app.ApplicationServices.GetService<ISocketConnectionManager>();

            app.MapWebSocket($"{options.BasePath}/ws",
                new SocketConnection((socket, text) => {
                    Console.WriteLine("Socket: {0}", text);
                    hub.SendJson(text);
                    return Task.CompletedTask;
                }),
                manager);

            app.UseMiddleware<DiagnosticsMiddleware>(options);

            return app;
        }

        public static JasperRegistry AddDiagnostics(this JasperRegistry registry)
        {
            registry.Logging.LogBusEventsWith<DiagnosticsBusLogger>();

            registry.Services.IncludeRegistry<JasperDiagnosticsServicesRegistry>();

            return registry;
        }
    }

    public class JasperDiagnosticsServicesRegistry : Registry
    {
        public JasperDiagnosticsServicesRegistry()
        {
            var assembly = typeof(IDiagnosticsClient).GetTypeInfo().Assembly;

            JsonSerialization.RegisterTypesFrom(assembly);

            ForSingletonOf<ISocketConnectionManager>().Use<SocketConnectionManager>();
            ForSingletonOf<IEventAggregator>().Use<EventAggregator>();
            ForSingletonOf<IDiagnosticsClient>().Use<DiagnosticsClient>();
            ForSingletonOf<IMessagingHub>().Use<MessagingHub>();

            Scan(_ =>
            {
                _.AssemblyContainingType<IDiagnosticsClient>();
                _.AddAllTypesOf<IListener>();
            });
        }
    }

    public static class HttpContextExtensions
    {
        public static async Task WriteHtml(this HttpContext context, HtmlTag tag)
        {
            var writer = new ResponseHtmlTextWriter(context.Response.Body);
            await tag.WriteHtml(writer);
        }
    }

    public class DiagnosticsListener : IListener, IListener<RequestInitialData>
    {
        private readonly IDiagnosticsClient _client;
        private readonly HandlerGraph _graph;

        public DiagnosticsListener(
            IDiagnosticsClient client,
            HandlerGraph graph)
        {
            _client = client;
            _graph = graph;
        }

        public void Receive<T>(T message)
        {
        }

        public void Receive(RequestInitialData message)
        {
            var chains = _graph.Chains.Select(c => ChainInfo.For(c));
            _client.Send(new InitialData(chains));
        }
    }

    public class RequestInitialData : ClientMessage
    {
        public RequestInitialData() : base("request-initial-data")
        {
        }
    }

    public class InitialData : ClientMessage
    {
        public InitialData(IEnumerable<ChainInfo> chains) : base("initial-data")
        {
            Chains = chains.ToArray();
        }

        public ChainInfo[] Chains { get; }
    }

    public class ChainInfo
    {
        public string TypeName { get; set; }

        public static ChainInfo For(HandlerChain chain)
        {
            return new ChainInfo { TypeName = chain.TypeName };
        }
    }
}

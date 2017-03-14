using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders.Embedded;
using Jasper.Diagnostics.Util;

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
            if(options.Mode == DiagnosticsMode.Production)
            {
                var assembly = typeof(DiagnosticsMiddleware).GetTypeInfo().Assembly;
                var provider = new EmbeddedFileProvider(assembly, $"{assembly.GetName().Name}.resources");
                var contents = provider.GetDirectoryContents("");

                foreach(var file in contents)
                {
                    Console.WriteLine(file.Name);
                }

                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = provider,
                    RequestPath = new PathString($"{options.BasePath}{DiagnosticsMiddleware.Resource_Root}")
                });
            }

            var manager = app.ApplicationServices.GetService<IWebSocketConnectionManager>();

            app.MapWebSocket($"{options.BasePath}/ws",
                new WebSocketHandler(
                    manager,
                    (socket, text) =>
                    {
                        return Task.CompletedTask;
                    },
                    async socket =>
                    {
                        var id = manager.GetId(socket);
                        Console.WriteLine($"[Socket]: Connected {id}");

                        await manager.SendToAllAsync($"yah! from {id}");
                    },
                    async socket =>
                    {
                        var id = manager.GetId(socket);
                        Console.WriteLine($"[Socket]: Disconnected {id}");
                        await manager.SendToAllAsync($"{id} disconnected");
                    })
                );

            app.UseMiddleware<DiagnosticsMiddleware>(options);

            return app;
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
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Jasper.Diagnostics.Util;

namespace Jasper.Diagnostics
{
    public class DiagnosticsSettings
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
        private readonly DiagnosticsSettings _settings;

        public DiagnosticsMiddleware(RequestDelegate next, DiagnosticsSettings settings)
        {
            _next = next;
            _settings = settings;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest ||
             !context.Request.Path.StartsWithSegments(_settings.BasePath))
            {
                await _next(context);
                return;
            }

            await buildLandingPage(context);
        }

        private async Task buildLandingPage(HttpContext context)
        {
            var document = new HtmlDocument { Title = _settings.PageTitle };

            document.Head.Append(new CssTag("https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.3.7/css/bootstrap.min.css"));
            document.Head.Append(new CssTag("https://cdnjs.cloudflare.com/ajax/libs/prism/1.6.0/themes/prism.min.css"));
            document.Head.Append(new CssTag("https://cdnjs.cloudflare.com/ajax/libs/prism/1.6.0/themes/prism-okaidia.min.css"));
            document.Head.Append(new CssTag("https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css"));

            document.Body.Append(new HtmlTag("div").Attr("id", "root"));

            var websocketUri = $"ws://localhost:{_settings.WebsocketPort}{_settings.BasePath}/ws";

            var initialData = new HtmlTag("script")
                .Attr("type", "text/javascript")
                .Text("{ var DiagnosticsSettings = { websocketAddress: '" + websocketUri + "' } }");

            document.Body.Append(initialData);

            document.Body.Append(new ScriptTag($"https://cdnjs.cloudflare.com/ajax/libs/prism/1.6.0/prism.js"));

            if(_settings.Mode == DiagnosticsMode.Development)
            {
                document.Body.Append(new ScriptTag($"/{Bundle_Name}"));
            }
            else
            {
                document.Body.Append(new ScriptTag($"{_settings.BasePath}{Resource_Root}/js/{Bundle_Name}"));
            }

            await context.WriteHtml(document);
        }
    }
}

using System.Linq;
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
        private readonly WebpackAssetCache _assets;

        public DiagnosticsMiddleware(RequestDelegate next, DiagnosticsSettings settings)
        {
            _next = next;
            _settings = settings;
            _assets = new WebpackAssetCache();
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

            if(_settings.Mode == DiagnosticsMode.Production)
            {
                document.Head.Append(cssTags());
            }

            document.Body.Append(new HtmlTag("div").Attr("id", "root"));

            var websocketUri = $"ws://localhost:{_settings.WebsocketPort}{pathFor(_settings.BasePath, "ws")}";

            var initialData = new HtmlTag("script")
                .Attr("type", "text/javascript")
                .Text("{ var DiagnosticsSettings = { websocketAddress: '" + websocketUri + "' } }");

            document.Body.Append(initialData);

            document.Body.Append(new ScriptTag($"https://cdnjs.cloudflare.com/ajax/libs/prism/1.6.0/prism.js"));

            if(_settings.Mode == DiagnosticsMode.Production)
            {
                document.Body.Append(scriptTags());
            }
            else
            {
                document.Body.Append(new ScriptTag($"/{Bundle_Name}"));
            }

            await context.WriteHtml(document);
        }

        private CssTag[] cssTags()
        {
            return _assets.CssFiles().Select(x => new CssTag(resourcePathFor(x))).ToArray();
        }

        private ScriptTag[] scriptTags()
        {
            return _assets.JavaScriptFiles().Select(x => new ScriptTag(resourcePathFor(x))).ToArray();
        }

        private string pathFor(params string[] urls)
        {
            return "".CombineUrl(urls);
        }

        private string resourcePathFor(params string[] urls)
        {
            return _settings.BasePath.CombineUrl(new [] {Resource_Root}.Concat(urls).ToArray());
        }
    }
}

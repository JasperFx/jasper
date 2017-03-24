using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Jasper.Diagnostics.Util;

namespace Jasper.Diagnostics
{
    public static class HttpContextExtensions
    {
        public static async Task WriteHtml(this HttpContext context, HtmlTag tag)
        {
            context.Response.ContentType = "text/html";
            var writer = new ResponseHtmlTextWriter(context.Response.Body);
            await tag.WriteHtml(writer);
        }
    }
}

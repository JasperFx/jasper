using System;
using System.Text;
using System.Threading.Tasks;
using Jasper.Conneg;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Model
{
    public abstract class RouteHandler
    {
        public abstract Task Handle(HttpContext input);

        public RouteChain Chain { get; set; }

        public static Task WriteText(string text, HttpResponse response)
        {
            response.Headers["content-type"] = "text/plain";
            response.Headers["content-length"] = text.Length.ToString();
            var bytes = Encoding.UTF8.GetBytes(text);
            return response.Body.WriteAsync(bytes, 0, bytes.Length);
        }

        public IMediaReader Reader { get; set; }
        public IMediaWriter Writer { get; set; }
        public ModelReader ConnegReader { get; set; }
        public ModelWriter ConnegWriter { get; set; }

        internal IMediaReader SelectReader(HttpRequest request)
        {
            return ConnegReader[request.ContentType];
        }

        internal IMediaWriter SelectWriter(HttpRequest request)
        {
            return ConnegWriter.ChooseWriter(request.Headers["accepts"]);
        }

    }
}

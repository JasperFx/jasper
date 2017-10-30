using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Baseline;
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

        public IMessageDeserializer Reader { get; set; }
        public IMessageSerializer Writer { get; set; }
        public ModelReader ConnegReader { get; set; }
        public ModelWriter ConnegWriter { get; set; }

        public IMessageDeserializer SelectReader(HttpRequest request)
        {
            return ConnegReader[request.ContentType];
        }

        public IMessageSerializer SelectWriter(HttpRequest request)
        {
            return ConnegWriter.ChooseWriter(request.Headers["accept"]);
        }

        public string ToRelativePath(string[] segments, int starting)
        {
            return segments.Skip(starting).Join("/");
        }

        public string[] ToPathSegments(string[] segments, int starting)
        {
            return segments.Skip(starting).ToArray();
        }
    }
}

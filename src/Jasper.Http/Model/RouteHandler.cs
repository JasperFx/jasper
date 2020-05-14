using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http.ContentHandling;
using Jasper.Serialization;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Model
{
    public abstract class RouteHandler
    {
        private const string ResourceNotFound = "Resource not found";


        public RouteChain Chain { get; set; }

        public IRequestReader Reader { get; set; }
        public IResponseWriter Writer { get; set; }
        public ReaderCollection<IRequestReader> ConnegReader { get; set; }
        public WriterCollection<IResponseWriter> ConnegWriter { get; set; }

        /// <summary>
        ///     Handles the actual HTTP request
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public abstract Task Handle(HttpContext httpContext);

        public Task UseWriter(object model, HttpResponse response)
        {
            var writer = Writer;

            return UseWriter(model, response, writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task UseWriter(object model, HttpResponse response, IResponseWriter writer)
        {
            if (model == null)
            {
                response.StatusCode = 404;
                response.ContentType = "text/plain";
                response.ContentLength = ResourceNotFound.Length;

                return response.WriteAsync(ResourceNotFound);
            }

            response.ContentType = writer.ContentType;
            return writer.WriteToStream(model, response);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WriteText(string text, HttpResponse response)
        {
            response.Headers["content-type"] = "text/plain";
            response.Headers["content-length"] = text.Length.ToString();
            var bytes = Encoding.UTF8.GetBytes(text);
            return response.Body.WriteAsync(bytes, 0, bytes.Length);
        }

        public IRequestReader SelectReader(HttpRequest request)
        {
            return ConnegReader[request.ContentType];
        }

        public IResponseWriter SelectWriter(HttpRequest request)
        {
            return ConnegWriter.ChooseWriter(request.Headers["accept"]);
        }

        public string ToRelativePath(HttpRequest request)
        {
            var items = ToPathSegments(request);
            return items.Join("/");
        }

        public string[] ToPathSegments(HttpRequest request)
        {
            var list = new List<string>();

            for (int i = 0; i < 12; i++)
            {
                if (request.RouteValues.TryGetValue("s" + i, out var item))
                {
                    list.Add((string)item);
                }
            }

            return list.ToArray();
        }
    }
}

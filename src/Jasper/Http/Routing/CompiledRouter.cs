using System;
using System.Threading.Tasks;
using Jasper.Http.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jasper.Http.Routing
{
    public abstract class CompiledRouter
    {
        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value.Trim().TrimStart('/').TrimEnd('/');

            try
            {
                if (path == string.Empty)
                {
                    await InvokeHome(context);
                }

                string[] segments = path.Split('/');
                context.SetSegments(segments);

                await Execute(context, segments);
            }
            catch (Exception e)
            {
                context.RequestServices.GetService<ILogger<HttpContext>>().LogError(new EventId(500), e, "Request Failed");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.ToString());
            }
        }

        public virtual Task InvokeHome(HttpContext context)
        {
            switch (context.Request.Method.ToUpper())
            {
                case "GET":
                    throw new NotImplementedException();
            }
            return NotFound(context);
        }

        public virtual Task Execute(HttpContext context, string[] segments)
        {
            //if (segments.)



            return NotFound(context);
        }

        public RequestDelegate NotFound { get; set; } = c =>
        {
            c.Response.StatusCode = 404;
            c.Response.Headers["status-description"] = "Resource Not Found";
            return c.Response.WriteAsync("Resource Not Found");
        };
    }
}

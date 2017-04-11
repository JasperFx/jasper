using System.Threading.Tasks;
using Jasper.Internal;
using Microsoft.AspNetCore.Http;

namespace JasperHttp.Model
{
    public abstract class RouteHandler : IHandler<HttpContext>
    {
        public abstract Task Handle(HttpContext input);

        public RouteChain Chain { get; set; }
    }
}
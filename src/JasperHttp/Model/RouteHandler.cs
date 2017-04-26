using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace JasperHttp.Model
{
    public abstract class RouteHandler
    {
        public abstract Task Handle(HttpContext input);

        public RouteChain Chain { get; set; }
    }
}

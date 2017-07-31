using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace Jasper.Http
{
    public class NulloServer : IServer
    {
        public void Dispose()
        {

        }

        public void Start<TContext>(IHttpApplication<TContext> application)
        {

        }

        public IFeatureCollection Features { get; } = new FeatureCollection();
    }
}
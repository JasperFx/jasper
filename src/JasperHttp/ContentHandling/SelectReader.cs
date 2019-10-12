using System.Reflection;
using Baseline.Reflection;
using JasperHttp.Model;
using LamarCodeGeneration.Frames;

namespace JasperHttp.ContentHandling
{
    public class SelectReader : MethodCall
    {
        private static readonly MethodInfo _method
            = ReflectionHelper.GetMethod<RouteHandler>(x => x.SelectReader(null));

        public SelectReader() : base(typeof(RouteHandler), _method)
        {
            IsLocal = true;
        }
    }
}

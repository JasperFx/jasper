using System.Reflection;
using Baseline.Reflection;
using JasperHttp.Model;
using LamarCodeGeneration.Frames;

namespace JasperHttp.ContentHandling
{
    public class SelectWriter : MethodCall
    {
        private static readonly MethodInfo _method
            = ReflectionHelper.GetMethod<RouteHandler>(x => x.SelectWriter(null));

        public SelectWriter() : base(typeof(RouteHandler), _method)
        {
            IsLocal = true;
        }
    }
}

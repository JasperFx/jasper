using System.Reflection;
using Baseline.Reflection;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
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

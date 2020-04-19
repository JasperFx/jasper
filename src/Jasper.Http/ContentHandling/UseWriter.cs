using System.Reflection;
using Baseline.Reflection;
using Jasper.Http.Model;
using LamarCodeGeneration.Frames;

namespace Jasper.Http.ContentHandling
{
    public class UseWriter : MethodCall
    {
        private static readonly MethodInfo _method =
            ReflectionHelper.GetMethod<RouteHandler>(x => x.UseWriter(null, null));

        public UseWriter(RouteChain chain) : base(typeof(IResponseWriter), _method)
        {
            Arguments[0] = chain.Action.ReturnVariable;

            IsLocal = true;
        }
    }

    public class UseChosenWriter : MethodCall
    {
        private static readonly MethodInfo _method = typeof(RouteHandler).GetMethod(nameof(RouteHandler.UseWriter),
            BindingFlags.Public | BindingFlags.Static);

        public UseChosenWriter(RouteChain chain) : base(typeof(IResponseWriter), _method)
        {
            Arguments[0] = chain.Action.ReturnVariable;

            IsLocal = true;
        }
    }
}

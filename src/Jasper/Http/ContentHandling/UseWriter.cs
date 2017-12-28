using System.Reflection;
using Baseline.Reflection;
using BlueMilk.Codegen;
using Jasper.Conneg;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class UseWriter : MethodCall
    {
        private static readonly MethodInfo _method = ReflectionHelper.GetMethod<IMessageSerializer>(x => x.WriteToStream(null, null));
        public UseWriter(RouteChain chain, bool isLocal    ) : base(typeof(IMessageSerializer), _method)
        {
            Variables[0] = chain.Action.ReturnVariable;

            if (isLocal)
            {
                Target = new Variable(typeof(IMessageSerializer), nameof(RouteHandler.Writer));
            }
        }
    }
}

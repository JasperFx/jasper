using System.Reflection;
using Baseline.Reflection;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Jasper.Messaging.Model
{
    public class CaptureCascadingMessages : MethodCall
    {
        private static readonly MethodInfo _method =
            ReflectionHelper.GetMethod<IAdvancedMessagingActions>(x => x.EnqueueCascading(null));


        public CaptureCascadingMessages(Variable messages) : base(typeof(IAdvancedMessagingActions),
            _method)
        {
            Arguments[0] = messages;
        }
    }
}

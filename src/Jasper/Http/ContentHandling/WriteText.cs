using System;
using System.Collections.Generic;
using System.Reflection;
using Jasper.Codegen;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class WriteText : IWriterRule
    {
        public bool Applies(RouteChain chain)
        {
            return chain.ResourceType == typeof(string);
        }

        public IEnumerable<Frame> DetermineWriters(RouteChain chain)
        {
            // TODO -- later, vary this for text/html or other things somehow
            yield return new CallWriteText(chain.Action.ReturnVariable);
        }
    }

    public class CallWriteText : MethodCall
    {
        private static readonly MethodInfo _method = typeof(RouteHandler).GetMethod(nameof(RouteHandler.WriteText));

        public CallWriteText(Variable text) : base(typeof(CallWriteText), _method)
        {
            Variables[0] = text;
            IsLocal = true;
        }
    }
}

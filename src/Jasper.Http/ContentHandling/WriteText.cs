using System;
using System.Collections.Generic;
using System.Reflection;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.Variables;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class WriteText : IWriterRule
    {
        public bool TryToApply(RouteChain chain)
        {
            if (chain.ResourceType != typeof(string)) return false;

            chain.Postprocessors.Add(new CallWriteText(chain.Action.ReturnVariable));

            return true;
        }
    }

    public class CallWriteText : MethodCall
    {
        private static readonly MethodInfo _method = typeof(RouteHandler).GetMethod(nameof(RouteHandler.WriteText));

        public CallWriteText(Variable text) : base(typeof(CallWriteText), _method)
        {
            Arguments[0] = text;
            IsLocal = true;
        }
    }
}

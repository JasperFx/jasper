using System.Reflection;
using Jasper.Http.Model;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Http.ContentHandling
{
    #region sample_WriteText
    public class WriteText : IWriterRule
    {
        public bool TryToApply(RouteChain chain)
        {
            if (chain.ResourceType != typeof(string))
            {
                return false;
            }

            chain.Postprocessors.Add(new CallWriteText(chain.Action.ReturnVariable));

            return true;
        }
    }
    #endregion

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

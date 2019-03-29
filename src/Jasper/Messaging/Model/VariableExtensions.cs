
using LamarCodeGeneration.Model;

namespace Jasper.Messaging.Model
{
    public static class VariableExtensions
    {
        public static void MarkAsNotCascading(this Variable variable)
        {
            if (!variable.Properties.ContainsKey(HandlerChain.NotCascading))
                variable.Properties.Add(HandlerChain.NotCascading, true);
        }

        public static bool IsNotCascadingMessage(this Variable variable)
        {
            return variable.Properties.ContainsKey(HandlerChain.NotCascading);
        }
    }
}

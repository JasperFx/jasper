using LamarCodeGeneration.Model;

namespace Jasper.Runtime.Handlers;

public static class VariableExtensions
{
    public static bool IsNotCascadingMessage(this Variable variable)
    {
        return variable.Properties.ContainsKey(HandlerChain.NotCascading);
    }
}

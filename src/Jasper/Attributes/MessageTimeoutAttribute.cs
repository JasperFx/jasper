using Jasper.Runtime.Handlers;
using LamarCodeGeneration;

namespace Jasper.Attributes;

public class MessageTimeoutAttribute : ModifyHandlerChainAttribute
{
    public MessageTimeoutAttribute(int timeoutInSeconds)
    {
        TimeoutInSeconds = timeoutInSeconds;
    }

    public int TimeoutInSeconds { get; }

    public override void Modify(HandlerChain chain, GenerationRules rules)
    {
        chain.ExecutionTimeoutInSeconds = TimeoutInSeconds;
    }
}

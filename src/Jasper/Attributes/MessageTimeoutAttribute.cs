using Jasper.Configuration;
using Jasper.Runtime.Handlers;
using Lamar;
using LamarCodeGeneration;

namespace Jasper.Attributes;

public class MessageTimeoutAttribute : ModifyHandlerChainAttribute
{
    public int TimeoutInSeconds { get; }

    public MessageTimeoutAttribute(int timeoutInSeconds)
    {
        TimeoutInSeconds = timeoutInSeconds;
    }

    public override void Modify(HandlerChain chain, GenerationRules rules)
    {
        chain.ExecutionTimeoutInSeconds = TimeoutInSeconds;
    }
}

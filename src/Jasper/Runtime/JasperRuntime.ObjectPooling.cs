using Microsoft.Extensions.ObjectPool;

namespace Jasper.Runtime;

public partial class JasperRuntime : PooledObjectPolicy<MessageContext>
{
    public override bool Return(MessageContext context)
    {
        context.ClearState();
        return true;
    }

    public override MessageContext Create()
    {
        return new MessageContext(this);
    }
}

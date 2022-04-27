using Microsoft.Extensions.ObjectPool;

namespace Jasper.Runtime;

public partial class JasperRuntime : PooledObjectPolicy<ExecutionContext>
{
    public override bool Return(ExecutionContext context)
    {
        context.ClearState();
        return true;
    }

    public override ExecutionContext Create()
    {
        return new ExecutionContext(this);
    }
}

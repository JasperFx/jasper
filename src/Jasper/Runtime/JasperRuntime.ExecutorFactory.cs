using System;
using Jasper.Runtime.Handlers;

namespace Jasper.Runtime;

public partial class JasperRuntime : IExecutorFactory
{
    IExecutor IExecutorFactory.BuildFor(Type messageType)
    {
        var handler = Handlers.HandlerFor(messageType);
        var executor = handler == null
            ? new NoHandlerExecutor(messageType, this)
            : (IExecutor)Executor.Build(this, Handlers, messageType);

        return executor;
    }
}

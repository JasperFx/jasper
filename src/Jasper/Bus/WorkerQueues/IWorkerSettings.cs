using System;

namespace Jasper.Bus.WorkerQueues
{
    public interface IWorkerSettings
    {
        IWorkerSettings MaximumParallelization(int maximumParallelHandlers    );
        IWorkerSettings Sequential();

        IWorkerSettings HandlesMessage<T>();
        IWorkerSettings HandleMessages(Func<Type, bool> filter);

        IWorkerSettings IsDurable();
    }
}
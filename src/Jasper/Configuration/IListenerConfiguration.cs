using System;
using System.Threading.Tasks.Dataflow;

namespace Jasper.Configuration
{
    public interface IListenerConfiguration<T>
    {
        /// <summary>
        ///     Specify the maximum number of threads that this worker queue
        ///     can use at one time
        /// </summary>
        /// <param name="maximumParallelHandlers"></param>
        /// <returns></returns>
        T MaximumThreads(int maximumParallelHandlers);

        /// <summary>
        ///     Forces this worker queue to use no more than one thread
        /// </summary>
        /// <returns></returns>
        T Sequential();

        /// <summary>
        ///     Force any messages enqueued to this worker queue to be durable
        /// </summary>
        /// <returns></returns>
        T Durably();

        /// <summary>
        /// By default, messages on this worker queue will not be persisted until
        /// being successfully handled
        /// </summary>
        /// <returns></returns>
        T Lightweight();


        /// <summary>
        /// Fine tune the internal message handling queue for this listener
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        T ConfigureExecution(Action<ExecutionDataflowBlockOptions> configure);


        /// <summary>
        /// Mark this listener as the preferred endpoint for replies from other systems
        /// </summary>
        /// <returns></returns>
        T UseForReplies();
    }

    public interface IListenerConfiguration : IListenerConfiguration<IListenerConfiguration>
    {

    }
}

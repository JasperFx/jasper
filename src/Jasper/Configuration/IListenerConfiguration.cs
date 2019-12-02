using System;
using System.Threading.Tasks.Dataflow;

namespace Jasper.Configuration
{
    public interface IListenerConfiguration
    {
        /// <summary>
        ///     Specify the maximum number of threads that this worker queue
        ///     can use at one time
        /// </summary>
        /// <param name="maximumParallelHandlers"></param>
        /// <returns></returns>
        IListenerConfiguration MaximumThreads(int maximumParallelHandlers);

        /// <summary>
        ///     Forces this worker queue to use no more than one thread
        /// </summary>
        /// <returns></returns>
        IListenerConfiguration Sequential();

        /// <summary>
        ///     Force any messages enqueued to this worker queue to be durable
        /// </summary>
        /// <returns></returns>
        IListenerConfiguration Durably();

        /// <summary>
        /// By default, messages on this worker queue will not be persisted until
        /// being successfully handled
        /// </summary>
        /// <returns></returns>
        IListenerConfiguration Lightweight();


        /// <summary>
        /// Fine tune the internal message handling queue for this listener
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        IListenerConfiguration ConfigureExecution(Action<ExecutionDataflowBlockOptions> configure);


    }
}

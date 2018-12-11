using System;

namespace Jasper.Messaging.WorkerQueues
{
    public interface IWorkerSettings
    {
        /// <summary>
        ///     Specify the maximum number of threads that this worker queue
        ///     can use at one time
        /// </summary>
        /// <param name="maximumParallelHandlers"></param>
        /// <returns></returns>
        IWorkerSettings MaximumParallelization(int maximumParallelHandlers);

        /// <summary>
        ///     Forces this worker queue to use no more than one thread
        /// </summary>
        /// <returns></returns>
        IWorkerSettings Sequential();

        /// <summary>
        ///     Direct messages of type T to be handled in this worker queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IWorkerSettings HandlesMessage<T>();

        /// <summary>
        ///     Directs messages that match the filter condition to be handled in
        ///     this worker queue
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        IWorkerSettings HandleMessages(Func<Type, bool> filter);

        /// <summary>
        ///     Force any messages enqueued to this worker queue to be durable
        /// </summary>
        /// <returns></returns>
        IWorkerSettings IsDurable();

        /// <summary>
        ///     By default, messages on this worker queue will not be durable
        /// </summary>
        /// <returns></returns>
        IWorkerSettings IsNotDurable();
    }
}

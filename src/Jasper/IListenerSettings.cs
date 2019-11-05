using System;

namespace Jasper
{
    public interface IListenerSettings
    {
        /// <summary>
        ///     Specify the maximum number of threads that this worker queue
        ///     can use at one time
        /// </summary>
        /// <param name="maximumParallelHandlers"></param>
        /// <returns></returns>
        IListenerSettings MaximumParallelization(int maximumParallelHandlers);

        /// <summary>
        ///     Forces this worker queue to use no more than one thread
        /// </summary>
        /// <returns></returns>
        IListenerSettings Sequential();

        /// <summary>
        ///     Force any messages enqueued to this worker queue to be durable
        /// </summary>
        /// <returns></returns>
        IListenerSettings IsDurable();

        /// <summary>
        ///     By default, messages on this worker queue will not be durable
        /// </summary>
        /// <returns></returns>
        IListenerSettings IsNotDurable();
    }
}

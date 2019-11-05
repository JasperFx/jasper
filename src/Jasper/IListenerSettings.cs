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
        ///     Direct messages of type T to be handled in this worker queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete("Eliminate with GH-557")]
        IListenerSettings HandlesMessage<T>();

        /// <summary>
        ///     Directs messages that match the filter condition to be handled in
        ///     this worker queue
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Obsolete("Eliminate with GH-557")]
        IListenerSettings HandleMessages(Func<Type, bool> filter);

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

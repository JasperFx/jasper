using System;
using System.Threading.Tasks;

namespace Jasper
{
    /// <summary>
    /// Slimmed down version of IMessageContext strictly for local command execution
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        ///     Invoke consumers for the relevant messages managed by the current
        ///     service bus instance. This happens immediately and on the current thread.
        ///     Error actions will not be executed and the message consumers will not be retried
        ///     if an error happens.
        /// </summary>
        Task Invoke(object message);

        /// <summary>
        ///     Invoke consumers for the relevant messages managed by the current
        ///     service bus instance and expect a response of type T from the processing. This happens immediately and on the
        ///     current thread.
        ///     Error actions will not be executed and the message consumers will not be retried
        ///     if an error happens.
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Invoke<T>(object message);

        /// <summary>
        ///     Enqueues the message locally. Uses the message type to worker queue routing to determine
        ///     whether or not the message should be durable or fire and forget
        /// </summary>
        /// <param name="message"></param>
        /// <param name="workerQueue">Optionally designate the name of the local worker queue</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Enqueue<T>(T message);

        /// <summary>
        ///     Enqueues the message locally to a specific worker queue.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="workerQueue">Optionally designate the name of the local worker queue</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Enqueue<T>(T message, string workerQueue);


        /// <summary>
        ///     Schedule a message to be processed in this application at a specified time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="executionTime"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<Guid> Schedule<T>(T message, DateTimeOffset executionTime);

        /// <summary>
        ///     Schedule a message to be processed in this application at a specified time with a delay
        /// </summary>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<Guid> Schedule<T>(T message, TimeSpan delay);
    }
}

using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging
{
    /// <summary>
    /// Slimmed down IMessageContext for stateless message sending and execution
    /// </summary>
    public interface IMessagePublisher : ICommandBus
    {
        /// <summary>
        ///     Publish a message to all known subscribers. Will throw an exception if there are no known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Send<T>(T message);

        /// <summary>
        ///     Send a message with explict control overrides to the Envelope. Will throw an exception if there are no known
        ///     subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Send<T>(T message, Action<Envelope> customize);

        /// <summary>
        ///     Publish a message to all known subscribers. Ignores the message if there are no known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Publish(Envelope envelope);

        /// <summary>
        ///     Publish a message to all known subscribers. Ignores the message if there are no known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Publish<T>(T message);

        /// <summary>
        ///     Send a message with explict control overrides to the Envelope. Ignores the message if there are no known
        ///     subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Publish<T>(T message, Action<Envelope> customize);

        /// <summary>
        ///     Send to a specific destination rather than running the routing rules
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination to send to</param>
        /// <param name="message"></param>
        Task Send<T>(Uri destination, T message);

        /// <summary>
        ///     Send a message that should be executed at the given time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        /// <typeparam name="T"></typeparam>
        Task ScheduleSend<T>(T message, DateTime time);

        /// <summary>
        ///     Send a message that should be executed after the given delay
        /// </summary>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        Task ScheduleSend<T>(T message, TimeSpan delay);

        /// <summary>
        ///     Send a message with the expectation of a response sent back to the global subscription
        ///     Uri of the logical service.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customization"></param>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        Task SendAndExpectResponseFor<TResponse>(object message, Action<Envelope> customization = null);


    }
}
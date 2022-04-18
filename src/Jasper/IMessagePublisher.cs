using System;
using System.Threading.Tasks;

namespace Jasper
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
        Task SendAsync<T>(T message);

        /// <summary>
        ///     Publish a message to all known subscribers. Ignores the message if there are no known subscribers
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task PublishEnvelopeAsync(Envelope envelope);

        /// <summary>
        ///     Publish a message to all known subscribers. Ignores the message if there are no known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task PublishAsync<T>(T message);


        /// <summary>
        /// Advanced publishing if you need absolute control over how a message is sent
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task<Guid> SendEnvelopeAsync(Envelope envelope);

        /// <summary>
        ///     Send a message with the expectation of a response sent back to the global subscription
        ///     Uri of the logical service.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customization"></param>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        Task SendAndExpectResponseForAsync<TResponse>(object message, Action<Envelope>? customization = null);


        /// <summary>
        /// Send a message to a specific topic name. This relies
        /// on having a backing transport endpoint that supports
        /// topic routing
        /// </summary>
        /// <param name="message"></param>
        /// <param name="topicName"></param>
        /// <returns></returns>
        Task SendToTopicAsync(object message, string topicName);


        /// <summary>
        ///     Send to a specific destination rather than running the routing rules
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination to send to</param>
        /// <param name="message"></param>
        Task SendToDestinationAsync<T>(Uri destination, T message);

        /// <summary>
        ///     Send a message that should be executed at the given time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        /// <typeparam name="T"></typeparam>
        Task ScheduleSendAsync<T>(T message, DateTimeOffset time);

        /// <summary>
        ///     Send a message that should be executed after the given delay
        /// </summary>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        Task ScheduleSendAsync<T>(T message, TimeSpan delay);
    }
}

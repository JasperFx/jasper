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
        Task Send<T>(T message);

        /// <summary>
        ///     Publish a message to all known subscribers. Ignores the message if there are no known subscribers
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task PublishEnvelope(Envelope envelope);

        /// <summary>
        ///     Publish a message to all known subscribers. Ignores the message if there are no known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Publish<T>(T message);


        /// <summary>
        /// Advanced publishing if you need absolute control over how a message is sent
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task<Guid> SendEnvelope(Envelope envelope);

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

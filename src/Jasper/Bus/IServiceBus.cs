using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;

namespace Jasper.Bus
{
    public interface IServiceBus
    {
        /// <summary>
        /// Loosely-coupled Request/Reply pattern
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        Task<TResponse> Request<TResponse>(object request, TimeSpan timeout = default(TimeSpan),  Action<Envelope> configure = null);

        /// <summary>
        /// Publish a message to all known subscribers. Will throw an exception if there are no known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Send<T>(T message);

        /// <summary>
        /// Send a message with explict control overrides to the Envelope. Will throw an exception if there are no known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Send<T>(T message, Action<Envelope> customize);

        /// <summary>
        /// Publish a message to all known subscribers. Ignores the message if there are no known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Publish(Envelope envelope);

        /// <summary>
        /// Publish a message to all known subscribers. Ignores the message if there are no known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Publish<T>(T message);

        /// <summary>
        /// Send a message with explict control overrides to the Envelope. Ignores the message if there are no known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Publish<T>(T message, Action<Envelope> customize);


        /// <summary>
        /// Send to a specific destination rather than running the routing rules
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination to send to</param>
        /// <param name="message"></param>
        Task Send<T>(Uri destination, T message);

        /// <summary>
        /// Invoke consumers for the relevant messages managed by the current
        /// service bus instance. This happens immediately and on the current thread.
        /// Error actions will not be executed and the message consumers will not be retried
        /// if an error happens.
        /// </summary>
        Task Invoke<T>(T message);

        /// <summary>
        /// Enqueues the message into the default loopback queue for this application
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Enqueue<T>(T message);

        /// <summary>
        /// Send a message that should be executed at the given time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        /// <typeparam name="T"></typeparam>
        Task DelaySend<T>(T message, DateTime time);

        /// <summary>
        /// Send a message that should be executed after the given delay
        /// </summary>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        Task DelaySend<T>(T message, TimeSpan delay);

        /// <summary>
        /// Send a message and await an acknowledgement that the
        /// message has been processed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendAndWait<T>(T message);

        /// <summary>
        /// Send a message to a specific destination and await an acknowledgment
        /// that the message has been processed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination to send to</param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendAndWait<T>(Uri destination, T message);

        /// <summary>
        /// Send a message with the expectation of a response sent back to the global subscription
        /// Uri of the logical service.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customization"></param>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        Task SendAndExpectResponseFor<TResponse>(object message, Action<Envelope> customization = null);


        /// <summary>
        /// Schedule a message to be processed in this application at a specified time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="executionTime"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<Guid> Schedule<T>(T message, DateTimeOffset executionTime);

        /// <summary>
        /// Schedule a message to be processed in this application at a specified time with a delay
        /// </summary>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<Guid> Schedule<T>(T message, TimeSpan delay);

        IEnumerable<Envelope> Outstanding { get; }
        bool EnlistedInTransaction { get; }
        IPersistence Persistence { get; }
        Task FlushOutstanding();
        void EnlistInTransaction(IEnvelopePersistor persistor);
    }
}

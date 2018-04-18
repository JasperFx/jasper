using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging
{
    public interface IMessageContext
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
        Task Invoke(object message);


        /// <summary>
        /// Invoke consumers for the relevant messages managed by the current
        /// service bus instance and expect a response of type T from the processing. This happens immediately and on the current thread.
        /// Error actions will not be executed and the message consumers will not be retried
        /// if an error happens.
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Invoke<T>(object message) where T : class;


        /// <summary>
        /// Enqueues the message locally. Uses the message type to worker queue routing to determine
        /// whether or not the message should be durable or fire and forget
        /// </summary>
        /// <param name="message"></param>
        /// <param name="workerQueue">Optionally designate the name of the local worker queue</param>
        /// <typeparam name="T"></typeparam>
        ///
        /// <returns></returns>
        Task Enqueue<T>(T message, string workerQueue = null);

        /// <summary>
        /// Enqueues the message locally in a durable manner
        /// </summary>
        /// <param name="message"></param>
        /// <param name="workerQueue">Optionally designate the name of the local worker queue</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task EnqueueDurably<T>(T message, string workerQueue = null);

        /// <summary>
        /// Enqueues the message locally in a fire and forget manner
        /// </summary>
        /// <param name="message"></param>
        /// <param name="workerQueue">Optionally designate the name of the local worker queue</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task EnqueueLightweight<T>(T message, string workerQueue = null);

        /// <summary>
        /// Send a message that should be executed at the given time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        /// <typeparam name="T"></typeparam>
        Task ScheduleSend<T>(T message, DateTime time);

        /// <summary>
        /// Send a message that should be executed after the given delay
        /// </summary>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        Task ScheduleSend<T>(T message, TimeSpan delay);

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

        /// <summary>
        /// The envelope being currently handled. This will only be non-null during
        /// the handling of a message
        /// </summary>
        Envelope Envelope { get; }



        /// <summary>
        /// If a messaging context is enlisted in a transaction, calling this
        /// method will force the context to send out any outstanding messages
        /// that were captured as part of processing the transaction
        /// </summary>
        /// <returns></returns>
        Task SendAllQueuedOutgoingMessages();

        /// <summary>
        /// Is the current context enlisted in a transaction?
        /// </summary>
        bool EnlistedInTransaction { get; }

        /// <summary>
        /// Enlist this context within some kind of existing business
        /// transaction so that messages are only sent if the transaction succeeds.
        /// Jasper's "Outbox" support
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task EnlistInTransaction(IEnvelopeTransaction transaction);


        /// <summary>
        /// Rarely used functions that are mostly consumed
        /// by Jasper itself
        /// </summary>
        IAdvancedMessagingActions Advanced { get; }


        /// <summary>
        /// Called by Jasper itself to mark this context as being part
        /// of a stateful saga
        /// </summary>
        /// <param name="sagaId"></param>
        void EnlistInSaga(object sagaId);
    }

    public interface IAdvancedMessagingActions
    {
        /// <summary>
        /// Current message persistence
        /// </summary>
        IDurableMessagingFactory Factory { get; }

        /// <summary>
        /// Send a failure acknowledgement back to the original
        /// sending service
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendFailureAcknowledgement(string message);

        /// <summary>
        /// Dumps any outstanding, cascading messages and requeues the current envelope (if any) into the local worker queue
        /// </summary>
        /// <returns></returns>
        Task Retry();

        /// <summary>
        /// Sends an acknowledgement back to the original sender
        /// </summary>
        /// <returns></returns>
        Task SendAcknowledgement();

        /// <summary>
        /// Current message logger
        /// </summary>
        IMessageLogger Logger { get; }

        /// <summary>
        /// Send a message envelope. Gives you complete power over how the message
        /// is delivered
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task<Guid> SendEnvelope(Envelope envelope);

        /// <summary>
        /// Enqueue a cascading message to the outstanding context transaction
        /// Can be either the message itself, any kind of ISendMyself object,
        /// or an IEnumerable<object>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task EnqueueCascading(object message);



    }
}

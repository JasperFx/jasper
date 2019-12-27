using System;
using System.Threading.Tasks;
using Jasper.Persistence.Durability;

namespace Jasper
{
    public interface IMessageContext : IMessagePublisher
    {
        /// <summary>
        /// Correlating identifier for the logical workflow. All envelopes sent or executed
        /// through this context will be tracked with this identifier. If this context is the
        /// result of a received message, this will be the original Envelope.CorrelationId
        /// </summary>
        Guid CorrelationId { get; }

        /// <summary>
        ///     The envelope being currently handled. This will only be non-null during
        ///     the handling of a message
        /// </summary>
        Envelope Envelope { get; }

        /// <summary>
        ///     Is the current context enlisted in a transaction?
        /// </summary>
        bool EnlistedInTransaction { get; }


        /// <summary>
        ///     Rarely used functions that are mostly consumed
        ///     by Jasper itself
        /// </summary>
        IAdvancedMessagingActions Advanced { get; }


        /// <summary>
        ///     If a messaging context is enlisted in a transaction, calling this
        ///     method will force the context to send out any outstanding messages
        ///     that were captured as part of processing the transaction
        /// </summary>
        /// <returns></returns>
        Task SendAllQueuedOutgoingMessages();

        /// <summary>
        ///     Enlist this context within some kind of existing business
        ///     transaction so that messages are only sent if the transaction succeeds.
        ///     Jasper's "Outbox" support
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task EnlistInTransaction(IEnvelopeTransaction transaction);



        /// <summary>
        ///     Called by Jasper itself to mark this context as being part
        ///     of a stateful saga
        /// </summary>
        /// <param name="sagaId"></param>
        void EnlistInSaga(object sagaId);
    }
}

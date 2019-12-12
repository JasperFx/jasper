using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Tracking
{
    public interface ITrackedSession
    {
        /// <summary>
        /// Completion status of the current messaging session
        /// </summary>
        TrackingStatus Status { get; }

        /// <summary>
        /// Finds a message of type T that was either sent, received,
        /// or executed during this session. This will throw an exception
        /// if there is more than one message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T FindSingleTrackedMessageOfType<T>();

        IEnumerable<object> UniqueMessages();
        IEnumerable<object> UniqueMessages(EventType eventType);
        T FindSingleTrackedMessageOfType<T>(EventType eventType);
        EnvelopeRecord[] FindEnvelopesWithMessageType<T>(EventType eventType);
        EnvelopeRecord[] FindEnvelopesWithMessageType<T>();
        EnvelopeRecord[] AllRecordsInOrder();
        bool HasNoRecordsOfAnyKind();
        EnvelopeRecord[] AllRecordsInOrder(EventType eventType);
    }
}

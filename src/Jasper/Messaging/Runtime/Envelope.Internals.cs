using System;
using System.Threading.Tasks;
using Jasper.Conneg;
using Jasper.Messaging.Transports;
using Jasper.Util;

namespace Jasper.Messaging.Runtime
{
    // Why is this a partial you ask?
    // The elements in this file are all things that only matter
    // inside the Jasper runtime so we can keep it out of the WireProtocol
    public partial class Envelope
    {
        public void MarkReceived(Uri uri, DateTime now, int currentNodeId)
        {
            ReceivedAt = uri;

            if (IsDelayed(now))
            {
                Status = TransportConstants.Scheduled;
                OwnerId = TransportConstants.AnyNode;
            }
            else
            {
                Status = TransportConstants.Incoming;
                OwnerId = currentNodeId;
            }
        }

        /// <summary>
        ///     Used internally to track the completion of an Envelope.
        /// </summary>
        public IMessageCallback Callback { get; set; }

        internal int SentAttempts { get; set; }

        private IMessageSerializer writer { get; set; }



        /// <summary>
        ///     Used by IMessageContext.Invoke<T> to denote the response type
        /// </summary>
        internal Type ResponseType { get; set; }

        /// <summary>
        ///     Also used by IMessageContext.Invoke<T> to catch the response
        /// </summary>
        internal object Response { get; set; }


        // TODO -- maybe hide these?
        /// <summary>
        ///     Status according to the message persistence
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     Node owner of this message. 0 denotes that no node owns this message
        /// </summary>
        public int OwnerId { get; set; }


        /// <summary>
        ///     Create a new Envelope that is a response to the current
        ///     Envelope
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Envelope ForResponse(object message)
        {
            var child = ForSend(message);
            child.CausationId = Id;

            if (message.GetType().ToMessageTypeName() == ReplyRequested)
            {
                child.Destination = ReplyUri;
                child.AcceptedContentTypes = AcceptedContentTypes;
            }

            return child;
        }

        internal Envelope ForSend(object message)
        {
            return new Envelope
            {
                Message = message,
                CorrelationId = CorrelationId.IsEmpty() ? Id : CorrelationId,
                CausationId = Id,
                SagaId = SagaId
            };
        }

        public Envelope Clone(IMessageSerializer writer)
        {
            var envelope = (Envelope)MemberwiseClone();
            envelope.writer = writer;

            return envelope;
        }

        public Envelope(object message, IMessageSerializer writer)
        {
            Message = message;
            this.writer = writer;
        }

        private bool _enqueued;

        internal ISubscriber Subscriber { get; set; }

        internal Task Send()
        {
            if (_enqueued) throw new InvalidOperationException("This envelope has already been enqueued");

            if (Subscriber == null) throw new InvalidOperationException("This envelope has not been routed");

            _enqueued = true;


            return Subscriber.Send(this);
        }

        internal Task QuickSend()
        {
            if (_enqueued) throw new InvalidOperationException("This envelope has already been enqueued");

            if (Subscriber == null) throw new InvalidOperationException("This envelope has not been routed");

            _enqueued = true;

            return Subscriber.QuickSend(this);
        }


        internal bool IsPing()
        {
            return MessageType == PingMessageType;
        }

        // TODO -- hide from public consumption
        /// <summary>
        ///     Used internally to ensure that the contained message has been serialized
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void EnsureData()
        {
            if (Data != null) return;

            if (_message == null)
                throw new InvalidOperationException("Cannot ensure data is present when there is no message");

            if (writer == null) throw new InvalidOperationException("No data or writer is known for this envelope");

            Data = writer.Write(_message);
        }


        /// <summary>
        ///     Create an Envelope for the given callback and raw data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Envelope ForData(byte[] data, IMessageCallback callback)
        {
            return new Envelope
            {
                Data = data,
                Callback = callback
            };
        }



        public static Envelope ForPing(Uri destination)
        {
            return new Envelope
            {
                MessageType = PingMessageType,
                Data = new byte[] {1, 2, 3, 4},
                ContentType = "jasper/ping",
                Destination = destination
            };
        }


    }
}

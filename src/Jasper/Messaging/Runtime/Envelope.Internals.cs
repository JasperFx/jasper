using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Conneg;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;

namespace Jasper.Messaging.Runtime
{
    public enum EnvelopeStatus
    {
        Outgoing,
        Scheduled,
        Incoming
    }

    // Why is this a partial you ask?
    // The elements in this file are all things that only matter
    // inside the Jasper runtime so we can keep it out of the WireProtocol
    public partial class Envelope
    {
        public static void MarkReceived(Envelope[] messages, Uri uri, DateTime now, int currentNodeId, out Envelope[] scheduled,
            out Envelope[] incoming)
        {
            foreach (var envelope in messages)
            {
                envelope.MarkReceived(uri, now, currentNodeId);
            }

            scheduled = messages.Where(x => x.Status == EnvelopeStatus.Scheduled).ToArray();
            incoming = messages.Where(x => x.Status == EnvelopeStatus.Incoming).ToArray();
        }

        public void MarkReceived(Uri uri, DateTime now, int currentNodeId)
        {
            ReceivedAt = uri;

            if (IsDelayed(now))
            {
                Status = EnvelopeStatus.Scheduled;
                OwnerId = TransportConstants.AnyNode;
            }
            else
            {
                Status = EnvelopeStatus.Incoming;
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


        /// <summary>
        ///     Status according to the message persistence
        /// </summary>
        public EnvelopeStatus Status { get; set; }

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

        internal ISendingAgent Sender { get; set; }

        internal Task Send()
        {
            if (_enqueued) throw new InvalidOperationException("This envelope has already been enqueued");

            if (Sender == null) throw new InvalidOperationException("This envelope has not been routed");

            _enqueued = true;


            return Sender.StoreAndForward(this);
        }

        internal Task QuickSend()
        {
            if (_enqueued) throw new InvalidOperationException("This envelope has already been enqueued");

            if (Sender == null) throw new InvalidOperationException("This envelope has not been routed");

            _enqueued = true;

            return Sender.EnqueueOutgoing(this);
        }


        public bool IsPing()
        {
            return MessageType == PingMessageType;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Serialization;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper
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
        internal static void MarkReceived(Envelope[] messages, Uri uri, DateTime now, int currentNodeId, out Envelope[] scheduled,
            out Envelope[] incoming)
        {
            foreach (var envelope in messages)
            {
                envelope.MarkReceived(uri, now, currentNodeId);
            }

            scheduled = messages.Where(x => x.Status == EnvelopeStatus.Scheduled).ToArray();
            incoming = messages.Where(x => x.Status == EnvelopeStatus.Incoming).ToArray();
        }

        internal void MarkReceived(Uri uri, DateTime now, int currentNodeId)
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
        internal IMessageCallback Callback { get; set; }

        internal int SentAttempts { get; set; }

        internal IMessageSerializer writer { get; set; }



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
        internal EnvelopeStatus Status { get; set; }

        /// <summary>
        ///     Node owner of this message. 0 denotes that no node owns this message
        /// </summary>
        internal int OwnerId { get; set; }


        /// <summary>
        ///     Create a new Envelope that is a response to the current
        ///     Envelope
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Envelope CreateForResponse(object message)
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

        internal Envelope CloneForWriter(IMessageSerializer writer)
        {
            var envelope = (Envelope)MemberwiseClone();
            envelope.Headers = new Dictionary<string, string>(Headers);
            envelope.writer = writer;

            return envelope;
        }

        internal Envelope(object message, IMessageSerializer writer)
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

        /// <summary>
        /// Is this envelope for a "ping" message used by Jasper to evaluate
        /// whether a sending endpoint can be restarted
        /// </summary>
        /// <returns></returns>
        internal bool IsPing()
        {
            return MessageType == PingMessageType;
        }


        /// <summary>
        ///     Create an Envelope for the given callback and raw data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        internal static Envelope ForData(byte[] data, IMessageCallback callback)
        {
            return new Envelope
            {
                Data = data,
                Callback = callback
            };
        }



        internal static Envelope ForPing(Uri destination)
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

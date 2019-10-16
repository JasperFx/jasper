﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Conneg;
using Jasper.Messaging.Transports;
using Jasper.Util;

namespace Jasper.Messaging.Runtime
{
    [MessageIdentity("envelope")]
    public partial class Envelope
    {
        private DateTimeOffset? _deliverBy;

        private DateTimeOffset? _executionTime;


        private object _message;

        public Envelope()
        {
        }



        public Envelope(object message)
        {
            Message = message;
        }

        public Envelope(object message, IMessageSerializer writer)
        {
            Message = message;
            this.writer = writer;
        }

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        /// <summary>
        ///     The raw, serialized message data
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        ///     The actual message to be sent or being received
        /// </summary>
        public object Message
        {
            get => _message;
            set
            {
                MessageType = value?.GetType().ToMessageTypeName();
                _message = value;
            }
        }

        /// <summary>
        ///     Number of times that Jasper has tried to process this message. Will
        ///     reflect the current attempt number
        /// </summary>
        public int Attempts { get; set; }

        /// <summary>
        ///     Used internally to track the completion of an Envelope.
        /// </summary>
        public IMessageCallback Callback { get; set; }


        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     Instruct Jasper to throw away this message if it is not successfully sent and processed
        ///     by the time specified
        /// </summary>
        public DateTimeOffset? DeliverBy
        {
            get => _deliverBy;
            set => _deliverBy = value?.ToUniversalTime();
        }

        internal int SentAttempts { get; set; }

        /// <summary>
        ///     Identifies the listener at which this envelope was received at
        /// </summary>
        public Uri ReceivedAt { get; set; }

        private IMessageSerializer writer { get; set; }

        /// <summary>
        ///     The name of the service that sent this envelope
        /// </summary>
        public string Source { get; set; }


        /// <summary>
        ///     Message type alias for the contents of this Envelpe
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        ///     Location where any replies should be sent
        /// </summary>
        public Uri ReplyUri { get; set; }

        /// <summary>
        ///     Mimetype of the serialized data
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///     Correlating identifier for the logical workflow or system action
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        ///     If this message is part of a stateful saga, this property identifies
        ///     the underlying saga state object
        /// </summary>
        public string SagaId { get; set; }

        /// <summary>
        ///     Id of the immediate message or workflow that caused this envelope to be sent
        /// </summary>
        public Guid CausationId { get; set; }

        /// <summary>
        ///     Location that this message should be sent
        /// </summary>
        public Uri Destination { get; set; }

        /// <summary>
        ///     Specifies the accepted content types for the requested reply
        /// </summary>
        public string[] AcceptedContentTypes { get; set; } = new string[0];

        /// <summary>
        ///     Specific message id for this envelope
        /// </summary>
        public Guid Id { get; set; } = CombGuidIdGeneration.NewGuid();

        /// <summary>
        ///     If specified, the message type alias for the reply message that is requested for this message
        /// </summary>
        public string ReplyRequested { get; set; }

        /// <summary>
        ///     Is an acknowledgement requested
        /// </summary>
        public bool AckRequested { get; set; }

        /// <summary>
        ///     Used by scheduled jobs to have this message processed by the receiving application at or after the designated time
        /// </summary>
        public DateTimeOffset? ExecutionTime
        {
            get => _executionTime;
            set => _executionTime = value?.ToUniversalTime();
        }

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
        ///     Used by IMessageContext.Invoke<T> to denote the response type
        /// </summary>
        internal Type ResponseType { get; set; }

        /// <summary>
        ///     Also used by IMessageContext.Invoke<T> to catch the response
        /// </summary>
        internal object Response { get; set; }


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

        public override string ToString()
        {
            var text = $"Envelope #{Id}";
            if (Message != null) text += $" ({Message.GetType().Name})";

            if (Source != null) text += $" from {Source}";

            if (Destination != null) text += $" to {Destination}";


            return text;
        }

        public Envelope Clone(IMessageSerializer writer)
        {
            var envelope = MemberwiseClone().As<Envelope>();
            envelope.writer = writer;

            return envelope;
        }

        /// <summary>
        ///     Set the DeliverBy property to have this message thrown away
        ///     if it cannot be sent before the alotted time
        /// </summary>
        /// <param name="span"></param>
        public void DeliverWithin(TimeSpan span)
        {
            DeliverBy = DateTime.UtcNow.Add(span);
        }

        protected bool Equals(Envelope other)
        {
            return Equals(Data, other.Data) && Equals(Message, other.Message) && Equals(Callback, other.Callback) &&
                   Equals(Headers, other.Headers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Envelope) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Data != null ? Data.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Callback != null ? Callback.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        ///     Should the processing of this message be scheduled for a later time
        /// </summary>
        /// <param name="utcNow"></param>
        /// <returns></returns>
        public bool IsDelayed(DateTime utcNow)
        {
            return ExecutionTime.HasValue && ExecutionTime.Value > utcNow;
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

        internal bool IsPing()
        {
            return MessageType == PingMessageType;
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

        /// <summary>
        ///     Has this envelope expired according to its DeliverBy value
        /// </summary>
        /// <returns></returns>
        public bool IsExpired()
        {
            return DeliverBy.HasValue && DeliverBy <= DateTime.UtcNow;
        }


        internal string GetMessageTypeName()
        {
            return Message?.GetType().Name ?? MessageType;
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

    }
}

using System;
using System.Collections.Generic;
using Baseline;

namespace JasperBus.Runtime
{
    public class Envelope : HeaderWrapper
    {
        public static readonly string OriginalIdKey = "original-id";
        public static readonly string IdKey = "id";
        public static readonly string ParentIdKey = "parent-id";
        public static readonly string ContentTypeKey = "content-type";
        public static readonly string SourceKey = "source";
        public static readonly string ChannelKey = "channel";
        public static readonly string ReplyRequestedKey = "reply-requested";
        public static readonly string ResponseIdKey = "response";
        public static readonly string DestinationKey = "destination";
        public static readonly string ReplyUriKey = "reply-uri";
        public static readonly string ExecutionTimeKey = "time-to-send";
        public static readonly string ReceivedAtKey = "received-at";
        public static readonly string AttemptsKey = "attempts";
        public static readonly string AckRequestedKey = "ack-requested";
        public static readonly string MessageTypeKey = "message-type";
        public static readonly string AcceptedContentTypesKey = "accepted-content-types";

        public byte[] Data;


        public object Message { get; set; }

        public int Attempts
        {
            get { return Headers.GetInt(AttemptsKey); }
            set { Headers.Set(AttemptsKey, value); }
        }

        public Envelope() : this(new Dictionary<string, string>())
        {
        }

        // TODO -- do routing slip tracking later

        public Envelope(IDictionary<string, string> headers)
        {
            Headers = headers;

            if (CorrelationId.IsEmpty())
            {
                CorrelationId = Guid.NewGuid().ToString();
            }

        }


        public Envelope(byte[] data, IDictionary<string, string> headers, IMessageCallback callback) : this(headers)
        {
            Data = data;
            Callback = callback;
        }

        public IMessageCallback Callback { get; set; }

        public bool MatchesResponse(object message)
        {
            return message.GetType().Name == ReplyRequested;
        }


        public virtual Envelope ForResponse(object message)
        {
            var child = ForSend(message);

            if (MatchesResponse(message))
            {
                child.Headers[ResponseIdKey] = CorrelationId;
                child.Destination = ReplyUri;
            }

            return child;
        }

        public virtual Envelope ForSend(object message)
        {
            return new Envelope
            {
                Message = message,
                OriginalId = OriginalId ?? CorrelationId,
                ParentId = CorrelationId
            };
        }

        public override string ToString()
        {
            var id = ResponseId.IsNotEmpty()
                ? "{0} in response to {1}".ToFormat(CorrelationId, ResponseId) : CorrelationId;

            return Message != null
                ? $"Envelope for message {Message} ({Message.GetType().Name}) w/ Id {id}"
                : "Envelope w/ Id {0}".ToFormat(id);
        }

        public Envelope Clone()
        {
            return new Envelope
            {
                Message = Message,
                Headers = Headers.Clone()
            };
        }

        /*
        public EnvelopeToken ToToken()
        {
            return new EnvelopeToken
            {
                Data = Data,
                Headers = Headers,
                MessageSource = _message
            };


        }
*/

        protected bool Equals(Envelope other)
        {
            return Equals(Data, other.Data) && Equals(Message, other.Message) && Equals(Callback, other.Callback) && Equals(Headers, other.Headers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Envelope) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Data != null ? Data.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Message != null ? Message.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Callback != null ? Callback.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static Envelope ForMessage(object message)
        {
            // TODO -- this will change
            return new Envelope(new byte[0], new Dictionary<string, string>(), null)
            {
                Message = message
            };
        }
    }

}
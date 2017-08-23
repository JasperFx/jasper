using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Bus.Transports;
using Jasper.Util;

namespace Jasper.Bus.Runtime
{
    public class Envelope : HeaderWrapper
    {
        private const string SentAttemptsHeaderKey = "sent-attempts";
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
        private object _message;


        public object Message
        {
            get => _message;
            set
            {
                MessageType = value?.GetType().ToMessageAlias();
                _message = value;
            }
        }

        public byte[] Identity() => Id.MessageIdentifier.ToByteArray();


        public int Attempts
        {
            get => Headers.GetInt(AttemptsKey);
            set => Headers.Set(AttemptsKey, value);
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
            return message.GetType().ToMessageAlias() == ReplyRequested;
        }


        public virtual Envelope ForResponse(object message)
        {
            var child = ForSend(message);



            if (MatchesResponse(message))
            {
                child.Headers[ResponseIdKey] = CorrelationId;
                child.Destination = ReplyUri;
                child.AcceptedContentTypes = AcceptedContentTypes;
                child.ResponseId = CorrelationId;
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
            var text = $"Envelope #{CorrelationId}";
            if (Message != null)
            {
                text += $" ({Message.GetType().Name})";
            }

            if (Source != null)
            {
                text += $" from {Source}";
            }

            if (Destination != null)
            {
                text += $" to {Destination}";
            }


            return text;
        }

        public Envelope Clone()
        {
            return new Envelope
            {
                Message = Message,
                Headers = Headers.Clone()
            };
        }


        private MessageId _id;
        private string _queue;

        public MessageId Id
        {
            get => _id ?? (_id = MessageId.GenerateRandom());
            set => _id = value;
        }

        public string Queue
        {
            get => _queue ?? Destination.QueueName();
            set => _queue = value;
        }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Obsolete("This is purely for backwards compatibility in wire format")]
        public string SubQueue { get; set; }

        public DateTime? DeliverBy
        {
            get
            {
                string headerValue;
                Headers.TryGetValue(DeliverByHeader, out headerValue);
                if (headerValue.IsNotEmpty())
                {
                    return DateTime.Parse(headerValue);
                }

                return null;
            }
            set
            {
                Headers.Set(DeliverByHeader, value.Value.ToString("o"));
            }
        }

        public int? MaxAttempts
        {
            get
            {
                return Headers.ContainsKey(MaxAttemptsHeader)
                    ? int.Parse(Headers[MaxAttemptsHeader])
                    : default(int?);
            }
            set
            {
                Headers.Set(MaxAttemptsHeader, value.ToString());
            }
        }

        public int SentAttempts
        {
            get
            {
                if (Headers.ContainsKey(SentAttemptsHeaderKey))
                {
                    return int.Parse(Headers[SentAttemptsHeaderKey]);
                }
                return 0;
            }
            set { Headers[SentAttemptsHeaderKey] = value.ToString(); }
        }

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

        public static string MaxAttemptsHeader = "max-delivery-attempts";
        public static string DeliverByHeader = "deliver-by";

        public Dictionary<string, string> WriteHeaders()
        {
            throw new NotImplementedException();
        }
    }

}

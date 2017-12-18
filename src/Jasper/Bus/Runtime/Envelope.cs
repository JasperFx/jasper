using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Tcp;
using Jasper.Conneg;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Bus.Runtime
{
    public partial class Envelope
    {
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

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

        public byte[] Identity() => EnvelopeVersionId.MessageIdentifier.ToByteArray();


        public int Attempts { get; set; }

        public Envelope()
        {
        }

        public Envelope(object message)
        {
            Message = message;
        }



        public Envelope(byte[] data, IMessageCallback callback)
        {
            Data = data;
            Callback = callback;
        }

        [JsonIgnore]
        public IMessageCallback Callback { get; set; }

        public bool MatchesResponse(object message)
        {
            return message.GetType().ToMessageAlias() == ReplyRequested;
        }


        public Envelope ForResponse(object message)
        {
            var child = ForSend(message);



            if (MatchesResponse(message))
            {
                child.ResponseId = Id;
                child.Destination = ReplyUri;
                child.AcceptedContentTypes = AcceptedContentTypes;
                child.ResponseId = Id;
            }

            return child;
        }

        public virtual Envelope ForSend(object message)
        {
            return new Envelope
            {
                Message = message,
                OriginalId = OriginalId.IsEmpty() ? Id : OriginalId,
                ParentId = Id
            };
        }

        public override string ToString()
        {
            var text = $"Envelope #{Id}";
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
            return MemberwiseClone().As<Envelope>();
        }


        private PersistedMessageId _id;
        private string _queue;
        private DateTimeOffset? _deliverBy;
        private DateTimeOffset? _executionTime;

        public PersistedMessageId EnvelopeVersionId
        {
            get => _id ?? (_id = PersistedMessageId.GenerateRandom());
            set => _id = value;
        }

        public string Queue
        {
            get => _queue ?? Destination.QueueName();
            set => _queue = value;
        }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // This is purely for backwards compatibility in wire format
        private string SubQueue { get; set; } = string.Empty;

        public DateTimeOffset? DeliverBy
        {
            get => _deliverBy;
            set => _deliverBy = value?.ToUniversalTime();
        }

        /// <summary>
        /// Set the DeliverBy property to have this message thrown away
        /// if it cannot be sent before the alotted time
        /// </summary>
        /// <param name="span"></param>
        public void DeliverWithin(TimeSpan span)
        {
            DeliverBy = DateTime.UtcNow.Add(span);
        }

        public int SentAttempts { get; set; }

        public Uri ReceivedAt { get; set; }

        internal IMessageSerializer Writer { get; set; }

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




        public void WriteData()
        {
            if (Writer == null || Message == null)
            {
                throw new InvalidOperationException("This envelope is missing a Writer and/or Message");
            }

            Data = Writer.Write(Message);
        }

        public string Source { get; set; }


        public string MessageType { get; set; }

        internal bool RequiresLocalReply { get; set; }

        public Uri ReplyUri { get; set; }

        public string ContentType { get; set; }

        public Guid OriginalId { get; set; }

        public Guid ParentId { get; set; }

        public Guid ResponseId { get; set; }

        public Uri Destination { get; set; }

        /// <summary>
        /// Read-only string representation of the destination Uri (for persistence)
        /// </summary>
        public string Address => Destination.ToString();

        public string[] AcceptedContentTypes { get; set; } = new string[0];

        public string Accepts => AcceptedContentTypes?.FirstOrDefault();


        public Guid Id { get; set; } = CombGuidIdGeneration.NewGuid();

        public string ReplyRequested { get; set; }

        public bool AckRequested { get; set; }

        public DateTimeOffset? ExecutionTime
        {
            get => _executionTime;
            set => _executionTime = value?.ToUniversalTime();
        }

        public string Status { get; set; }

        public int OwnerId { get; set; } = 0;

        internal MessageRoute Route { get; set; }

        public bool IsDelayed(DateTime utcNow)
        {

            return ExecutionTime.HasValue && ExecutionTime.Value > utcNow;
        }

        public void EnsureData()
        {
            if (Data != null) return;

            if (_message == null) throw new InvalidOperationException("Cannot ensure data is present when there is no message");

            if (Writer == null) throw new InvalidOperationException("No data or writer is known for this envelope");

            Data = Writer.Write(_message);
        }

        public bool IsPing()
        {
            return MessageType == TransportConstants.PingMessageType;
        }

        public static Envelope ForPing()
        {
            return new Envelope
            {
                MessageType = TransportConstants.PingMessageType,
                Data = new byte[]{1,2,3,4}
            };
        }

        public bool IsExpired()
        {
            return DeliverBy.HasValue && DeliverBy <= DateTime.UtcNow;
        }

        private bool _enqueued = false;

        public Task Send()
        {
            if (_enqueued)
            {
                throw new InvalidOperationException("This envelope has already been enqueued");
            }

            if (Route == null)
            {
                throw new InvalidOperationException("This envelope has not been routed");
            }

            _enqueued = true;



            return Route.Channel.Send(this);
        }

        public Task QuickSend()
        {
            if (_enqueued)
            {
                throw new InvalidOperationException("This envelope has already been enqueued");
            }

            if (Route == null)
            {
                throw new InvalidOperationException("This envelope has not been routed");
            }

            _enqueued = true;

            return Route.Channel.QuickSend(this);
        }
    }

}

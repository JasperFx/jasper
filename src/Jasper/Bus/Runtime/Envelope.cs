using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Core;
using Jasper.Conneg;
using Jasper.Util;

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
                child.ResponseId = CorrelationId;
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
            return MemberwiseClone().As<Envelope>();
        }


        private PersistedMessageId _id;
        private string _queue;
        private DateTime? _deliverBy;
        private DateTime? _executionTime;

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

        [Obsolete("This is purely for backwards compatibility in wire format")]
        public string SubQueue { get; set; }

        public DateTime? DeliverBy
        {
            get { return _deliverBy; }
            set { _deliverBy = value?.ToUniversalTime(); }
        }

        public int SentAttempts { get; set; }

        public Uri ReceivedAt { get; set; }

        public IMessageSerializer Writer { get; set; }

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

        public Uri ReplyUri { get; set; }

        public string ContentType { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string OriginalId { get; set; }

        public string ParentId { get; set; }

        public string ResponseId { get; set; }

        public Uri Destination { get; set; }

        public string[] AcceptedContentTypes { get; set; } = new string[0];

        public string Accepts => AcceptedContentTypes?.FirstOrDefault();


        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

        public string ReplyRequested { get; set; }

        public bool AckRequested { get; set; }

        public DateTime? ExecutionTime
        {
            get => _executionTime;
            set => _executionTime = value?.ToUniversalTime();
        }

        public bool IsDelayed(DateTime utcNow)
        {
            return ExecutionTime.HasValue && ExecutionTime.Value > utcNow;
        }
    }

}

using System;

namespace Jasper
{
    public class FailureAcknowledgement
    {
        public Guid CorrelationId { get; set; }
        public string Message { get; set; }

        protected bool Equals(FailureAcknowledgement other)
        {
            return Equals(CorrelationId, other.CorrelationId) && string.Equals(Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FailureAcknowledgement) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((CorrelationId != null ? CorrelationId.GetHashCode() : 0) * 397) ^
                       (Message != null ? Message.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"Failure acknowledgement for {CorrelationId} / '{Message}'";
        }
    }
}

using System;

namespace Jasper.Messaging.Runtime
{
    public class Acknowledgement
    {
        public Guid CorrelationId { get; set; }

        protected bool Equals(Acknowledgement other)
        {
            return string.Equals(CorrelationId, other.CorrelationId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Acknowledgement) obj);
        }

        public override int GetHashCode()
        {
            return (CorrelationId != null ? CorrelationId.GetHashCode() : 0);
        }

    }
}
using System;

namespace Jasper
{
    public class ErrorReport
    {
        public const string ExceptionDetected = "Exception Detected";

        public ErrorReport()
        {
        }

        public ErrorReport(Envelope envelope, Exception ex)
        {
            ExceptionText = ex.ToString();
            ExceptionMessage = ex.Message;
            ExceptionType = ex.GetType().FullName;
            Explanation = ExceptionDetected;

            try
            {
                RawData = envelope.Serialize();
            }
            catch (Exception)
            {
                // Nothing
            }

            MessageType = envelope.MessageType;
            Source = envelope.Source;
            Id = envelope.Id;
        }


        public Guid Id { get; set; }

        public string Source { get; set; }
        public string MessageType { get; set; }


        public byte[] RawData { get; set; }


        public string Explanation { get; set; }

        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionText { get; set; }


        public Envelope RebuildEnvelope()
        {
            return Envelope.Deserialize(RawData);
        }

        protected bool Equals(ErrorReport other)
        {
            return Source == other.Source && MessageType == other.MessageType && ExceptionType == other.ExceptionType && ExceptionMessage == other.ExceptionMessage && ExceptionText == other.ExceptionText;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ErrorReport) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Source != null ? Source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MessageType != null ? MessageType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExceptionType != null ? ExceptionType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExceptionMessage != null ? ExceptionMessage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExceptionText != null ? ExceptionText.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}

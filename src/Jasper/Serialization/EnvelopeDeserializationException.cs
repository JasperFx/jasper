using System;

namespace Jasper.Serialization
{
    public class EnvelopeDeserializationException : Exception
    {
        public EnvelopeDeserializationException(string message) : base(message)
        {
        }

        public EnvelopeDeserializationException(string message, Exception innerException) : base(message,
            innerException)
        {
        }

        public static EnvelopeDeserializationException ForReadFailure(Envelope envelope, Exception inner)
        {
            var message =
                $"Envelope deserialization failed! ContentType: {envelope.ContentType}, MessageType: {envelope.MessageType}";
            return new EnvelopeDeserializationException(message, inner);
        }
    }
}

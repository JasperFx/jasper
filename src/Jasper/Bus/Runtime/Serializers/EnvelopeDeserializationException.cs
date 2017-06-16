using System;

namespace Jasper.Bus.Runtime.Serializers
{
    public class EnvelopeDeserializationException : Exception
    {
        public EnvelopeDeserializationException(string message) : base(message)
        {
        }

        public EnvelopeDeserializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
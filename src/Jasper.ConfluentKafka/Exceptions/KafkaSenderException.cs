using System;
using Confluent.Kafka;

namespace Jasper.ConfluentKafka.Exceptions
{
    public class KafkaSenderException : ApplicationException
    {
        public Error Error { get; }

        public KafkaSenderException(Error error) : base(error.Reason)
        {
            Error = error;
        }
    }
}

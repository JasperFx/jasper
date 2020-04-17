using System;

namespace Jasper.ConfluentKafka.Exceptions
{
    public class UnsupportedFeatureException : ApplicationException
    {
        public string Feature { get; }
        public UnsupportedFeatureException(string feature)
        : base ($"Confluent Kafka Transport does not support {feature}")
        {
            Feature = feature;
        }
    }
}

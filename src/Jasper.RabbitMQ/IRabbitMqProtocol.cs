using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    // SAMPLE: IRabbitMqProtocol
    /// <summary>
    /// Used to adapt Jasper to interact with non-Jasper applications through Rabbit MQ
    /// queues by mapping from Jasper's Envelope to the Rabbit MQ header structure
    /// </summary>
    public interface IRabbitMqProtocol
    {
        /// <summary>
        /// Transfer information from Jasper's Envelope to the Rabbit MQ properties
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="properties"></param>
        void WriteFromEnvelope(Envelope envelope, IBasicProperties properties);

        /// <summary>
        /// Create an Envelope object from the raw message data and the header
        /// values in the properties
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="properties"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        void ReadIntoEnvelope(Envelope envelope, IBasicProperties properties, byte[] body);
    }

    // ENDSAMPLE
}

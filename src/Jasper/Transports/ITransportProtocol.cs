namespace Jasper.Transports
{
    public interface ITransportProtocol<TTransportMsg>
    {
        /// <summary>
        /// Creates a transport message object from a Jasper Envelope
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        TTransportMsg WriteFromEnvelope(Envelope envelope);

        /// <summary>
        /// Creates an Envelope from the incoming transport message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Envelope ReadEnvelope(TTransportMsg message);
    }
}

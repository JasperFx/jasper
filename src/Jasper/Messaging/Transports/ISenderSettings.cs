namespace Jasper.Messaging.Transports
{
    public interface ISenderSettings
    {
        /// <summary>
        /// Force any messages enqueued to be sent by this sender to be durable
        /// </summary>
        /// <returns></returns>
        ISenderSettings Durably();

        /// <summary>
        /// By default, messages on this sender will not be persisted until
        /// being successfully handled
        /// </summary>
        /// <returns></returns>
        ISenderSettings Lightweight();
    }
}

namespace Jasper.Configuration
{
    public interface ISubscriberConfiguration
    {
        /// <summary>
        ///     Force any messages enqueued to this worker queue to be durable
        /// </summary>
        /// <returns></returns>
        ISubscriberConfiguration Durably();

        /// <summary>
        /// By default, messages on this worker queue will not be persisted until
        /// being successfully handled
        /// </summary>
        /// <returns></returns>
        ISubscriberConfiguration Lightweight();


    }
}

namespace Jasper.Configuration
{
    public interface ISubscriberConfiguration<T> where T : ISubscriberConfiguration<T>
    {
        /// <summary>
        ///     Force any messages enqueued to this worker queue to be durable
        /// </summary>
        /// <returns></returns>
        T Durably();

        /// <summary>
        /// By default, messages on this worker queue will not be persisted until
        /// being successfully handled
        /// </summary>
        /// <returns></returns>
        T Lightweight();
    }

    public interface ISubscriberConfiguration : ISubscriberConfiguration<ISubscriberConfiguration>
    {

    }
}

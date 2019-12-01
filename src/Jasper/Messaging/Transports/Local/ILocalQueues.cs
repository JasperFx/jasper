namespace Jasper.Messaging.Transports.Local
{
    public interface ILocalQueues
    {
        /// <summary>
        /// Retrieve the configuration for a local queue by name. Case insensitive.
        /// Will create a new queue if one with this name does not already exist
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        LocalQueueSettings ByName(string queueName);

        /// <summary>
        /// Access the configuration for the default local queue
        /// </summary>
        /// <returns></returns>
        LocalQueueSettings Default();


    }
}

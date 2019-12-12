using System;
using Jasper.Messaging.Configuration;

namespace Jasper.Configuration
{
    public interface IEndpoints
    {
        /// <summary>
        ///     Directs Jasper to set up an incoming listener fvoidor the given Uri
        /// </summary>
        /// <param name="uri"></param>
        IListenerConfiguration ListenForMessagesFrom(Uri uri);

        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        IListenerConfiguration ListenForMessagesFrom(string uriString);

        /// <summary>
        ///     Directs the application to listen at the designated port in a
        ///     fast, but non-durable way
        /// </summary>
        /// <param name="port"></param>
        IListenerConfiguration ListenAtPort(int port);

        /// <summary>
        /// Create a message publishing rule for a subset of message types
        /// and one or more messaging endpoints
        /// </summary>
        /// <param name="configuration"></param>
        void Publish(Action<PublishingExpression> configuration);

        /// <summary>
        /// Publish all message types to a messaging endpoint
        /// </summary>
        /// <returns></returns>
        IPublishToExpression PublishAllMessages();

        /// <summary>
        /// Retrieve the configuration for a local queue by name. Case insensitive.
        /// Will create a new queue if one with this name does not already exist
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        IListenerConfiguration LocalQueue(string queueName);

        /// <summary>
        /// Access the configuration for the default local queueLocalQueueSettings
        /// </summary>
        /// <value></value>
        IListenerConfiguration DefaultLocalQueue { get; }


        /// <summary>
        /// This should probably *only* be used in development or testing
        /// to latch all outgoing message sending
        /// </summary>
        void StubAllExternallyOutgoingEndpoints();
    }
}

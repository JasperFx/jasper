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
        IPublishToExpression PublishAll();


    }
}

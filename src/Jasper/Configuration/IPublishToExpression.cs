using System;
using Jasper.Runtime.Routing;

namespace Jasper.Configuration
{
    public interface IPublishToExpression
    {
        /// <summary>
        /// All matching records are to be sent to the configured subscriber
        /// by Uri
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        ISubscriberConfiguration To(Uri uri);

        /// <summary>
        /// Send all the matching messages to the designated Uri string
        /// </summary>
        /// <param name="uriString"></param>
        /// <returns></returns>
        ISubscriberConfiguration To(string uriString);

        /// <summary>
        ///     Publish the designated message types using Jasper's lightweight
        ///     TCP transport locally to the designated port number
        /// </summary>
        /// <param name="port"></param>
        ISubscriberConfiguration ToPort(int port);

        /// <summary>
        ///     Publish the designated message types to the named
        ///     local queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        IListenerConfiguration ToLocalQueue(string queueName);

        /// <summary>
        ///     Publish messages using the TCP transport to the specified
        ///     server name and port
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        ISubscriberConfiguration ToServerAndPort(string hostName, int port);

        /// <summary>
        ///     Publishes the matching messages locally to the default
        ///     local queue
        /// </summary>
        IListenerConfiguration Locally();

        /// <summary>
        /// Use a routing rule "Subscriber" as a recipient. This is used
        /// by Jasper's topic routing
        /// </summary>
        /// <param name="subscriber"></param>
        void ViaRouter(Subscriber subscriber);
    }
}

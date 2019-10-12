using System;

namespace Jasper.Configuration
{
    public interface ITransportsExpression
    {
        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        /// <param name="uri"></param>
        void ListenForMessagesFrom(Uri uri);

        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        void ListenForMessagesFrom(string uriString);

        /// <summary>
        ///     Toggle a transport type to enabled. All transports are enabled by default though
        /// </summary>
        /// <param name="protocol"></param>
        void EnableTransport(string protocol);

        /// <summary>
        ///     Disable a single transport by protocol
        /// </summary>
        /// <param name="protocol"></param>
        void DisableTransport(string protocol);


    }
}
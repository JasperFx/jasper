using System;
using Jasper.Http;

namespace Jasper.Messaging.Transports.Configuration
{
    public interface ITransportsExpression
    {
        /// <summary>
        /// Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        /// <param name="uri"></param>
        void ListenForMessagesFrom(Uri uri);

        /// <summary>
        /// Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        void ListenForMessagesFrom(string uriString);

        /// <summary>
        /// Use the designated Uri for sending messages that do not
        /// have explicit routing
        /// </summary>
        /// <param name="uriString"></param>
        void DefaultIs(string uriString);

        /// <summary>
        /// Use the designated Uri for sending messages that do not
        /// have explicit routing
        /// </summary>
        /// <param name="uri"></param>
        void DefaultIs(Uri uri);

        /// <summary>
        /// All messages sent through the service bus will be handled locally
        /// </summary>
        void ExecuteAllMessagesLocally();

        /// <summary>
        /// Toggle a transport type to enabled. All transports are enabled by default though
        /// </summary>
        /// <param name="protocol"></param>
        void EnableTransport(string protocol);

        /// <summary>
        /// Disable a single transport by protocol
        /// </summary>
        /// <param name="protocol"></param>
        void DisableTransport(string protocol);



        IHttpTransportConfiguration Http { get; }

    }

    public static class TransportsExpressionExtensions
    {
        /// <summary>
        /// Directs the application to listen at the designated port in a
        /// fast, but non-durable way
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="port"></param>
        public static void LightweightListenerAt(this ITransportsExpression expression, int port)
        {
            expression.ListenForMessagesFrom($"tcp://localhost:{port}");
        }

        /// <summary>
        /// Directs the application to listen at the designated port in a
        /// durable way
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="port"></param>
        public static void DurableListenerAt(this ITransportsExpression expression, int port)
        {
            expression.ListenForMessagesFrom($"tcp://localhost:{port}/durable");
        }
    }
}

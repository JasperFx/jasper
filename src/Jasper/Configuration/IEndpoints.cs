using System;

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
    }
}

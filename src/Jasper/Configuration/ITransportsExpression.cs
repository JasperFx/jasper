using System;

namespace Jasper.Configuration
{
    public interface ITransportsExpression
    {
        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        /// <param name="uri"></param>
        IListenerSettings ListenForMessagesFrom(Uri uri);

        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        IListenerSettings ListenForMessagesFrom(string uriString);

    }
}

using System;

namespace Jasper.Configuration
{
    public interface ITransportsExpression
    {
        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        /// <param name="uri"></param>
        [Obsolete("remove and replace with per-transport methods")]
        IListenerSettings ListenForMessagesFrom(Uri uri);

        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        [Obsolete("remove and replace with per-transport methods")]
        IListenerSettings ListenForMessagesFrom(string uriString);

    }
}

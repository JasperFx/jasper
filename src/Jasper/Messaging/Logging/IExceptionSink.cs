using System;

namespace Jasper.Messaging.Logging
{
    public interface IExceptionSink
    {
        /// <summary>
        /// Catch all hook for any exceptions encountered by the messaging
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="correlationId"></param>
        /// <param name="message"></param>
        void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:");

    }
}

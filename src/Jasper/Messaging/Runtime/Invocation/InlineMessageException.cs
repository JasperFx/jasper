using System;

namespace Jasper.Messaging.Runtime.Invocation
{
    public class InlineMessageException : Exception
    {
        public InlineMessageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
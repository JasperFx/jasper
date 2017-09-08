using System;

namespace Jasper.Bus.Runtime.Invocation
{
    public class InlineMessageException : Exception
    {
        public InlineMessageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
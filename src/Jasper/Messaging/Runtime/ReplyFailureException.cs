using System;

namespace Jasper.Messaging.Runtime
{
    public class ReplyFailureException : Exception
    {
        public ReplyFailureException(string message) : base(message)
        {
        }
    }
}
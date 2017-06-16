using System;

namespace JasperBus.Runtime
{
    public class ReplyFailureException : Exception
    {
        public ReplyFailureException(string message) : base(message)
        {
        }
    }
}
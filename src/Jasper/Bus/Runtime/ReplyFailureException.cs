using System;

namespace Jasper.Bus.Runtime
{
    public class ReplyFailureException : Exception
    {
        public ReplyFailureException(string message) : base(message)
        {
        }
    }
}
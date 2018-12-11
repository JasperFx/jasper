using System;
using System.Linq;
using Baseline;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Transports.Tcp
{
    public class MessageFailureException : Exception
    {
        public MessageFailureException(Envelope[] messages, Exception innerException) : base(
            $"SEE THE INNER EXCEPTION -- Failed on messages {messages.Select(x => x.ToString()).Join(", ")}",
            innerException)
        {
        }
    }
}

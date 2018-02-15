using System;

namespace Jasper.Messaging
{
    public class UnknownTransportException : Exception
    {
        public UnknownTransportException(string message) : base(message)
        {

        }
    }
}
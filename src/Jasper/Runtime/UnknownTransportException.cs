using System;

namespace Jasper.Runtime
{
    public class UnknownTransportException : Exception
    {
        public UnknownTransportException(string message) : base(message)
        {
        }
    }
}

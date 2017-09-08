using System;

namespace Jasper.Bus
{
    public class UnknownTransportException : Exception
    {
        public UnknownTransportException(string message) : base(message)
        {

        }
    }
}
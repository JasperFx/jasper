using System;

namespace Jasper.Messaging.Runtime
{
    public class NoRoutesException : Exception
    {
        public NoRoutesException(Envelope envelope) : base($"Could not determine any valid routes for {envelope}")
        {
        }
    }
}
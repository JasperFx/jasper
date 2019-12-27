using System;

namespace Jasper.Runtime.Routing
{
    public class NoRoutesException : Exception
    {
        public NoRoutesException(Envelope envelope) : base($"Could not determine any valid routes for {envelope}")
        {
        }
    }
}

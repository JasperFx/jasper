using System;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.ErrorHandling
{
    public interface IExceptionMatch
    {
        bool Matches(Envelope envelope, Exception ex);
    }
}
